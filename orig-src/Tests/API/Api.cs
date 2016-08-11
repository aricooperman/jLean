/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using QuantConnect.Api;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;

package com.quantconnect.lean.Tests.API
{
    [TestFixture, Category("TravisExclude")]
    class RestApiTests
    {
        //Test Authentication Credentials
        private int _testAccount = 1;
        private String _testToken = "ec87b337ac970da4cbea648f24f1c851";

        /// <summary>
        /// Test successfully authenticates with the API using valid credentials.
        /// </summary>
        [Test]
        public void AuthenticatesSuccessfully()
        {
            connection = new ApiConnection(_testAccount, _testToken);
            Assert.IsTrue(connection.Connected);
        }

        /// <summary>
        /// Rejects invalid credentials
        /// </summary>
        [Test]
        public void RejectsInvalidCredentials()
        {
            connection = new ApiConnection(_testAccount, "");
            Assert.IsFalse(connection.Connected);
        }

        /// <summary>
        /// Tests all the API methods linked to a project id.
        ///  - Creates project,
        ///  - Adds files to project,
        ///  - Updates the files, makes sure they are still present,
        ///  - Builds the project, 
        /// </summary>
        [Test]
        public void CreatesProjectCompilesAndBacktestsProject()
        {
            // Initialize the test:
            api = CreateApiAccessor();
            sources = new List<TestAlgorithm>()
            {
                new TestAlgorithm(Language.CSharp, "main.cs", File.ReadAllText("../../../Algorithm.CSharp/BasicTemplateAlgorithm.cs")),
                new TestAlgorithm(Language.FSharp, "main.fs", File.ReadAllText("../../../Algorithm.FSharp/BasicTemplateAlgorithm.fs")),
                new TestAlgorithm(Language.Python, "main.py", File.ReadAllText("../../../Algorithm.Python/BasicTemplateAlgorithm.py"))
            };

            foreach (source in sources)
            {
                // Test create a new project successfully
                name = DateTime.UtcNow.toString("u") + " Test " + _testAccount + " Lang " + source.Language;
                project = api.CreateProject(name, source.Language);
                Assert.IsTrue(project.Success);
                Assert.IsTrue(project.ProjectId > 0);
                Console.WriteLine("API Test: {0} Project created successfully", source.Language);

                // Gets the list of projects from the account. 
                // Should at least be the one we created.
                projects = api.ProjectList();
                Assert.IsTrue(projects.Success);
                Assert.IsTrue(projects.Projects.Count >= 1);
                Console.WriteLine("API Test: All Projects listed successfully");

                // Test read back the project we just created
                readProject = api.ReadProject(project.ProjectId);
                Assert.IsTrue(readProject.Success);
                Assert.IsTrue(readProject.Files.Count == 0);
                Assert.IsTrue(readProject.Name == name);
                Console.WriteLine("API Test: {0} Project read successfully", source.Language);

                // Test set a project file for the project
                files = new List<ProjectFile>
                {
                    new ProjectFile { Name = source.Name, Code = source.Code }
                };
                updateProject = api.UpdateProject(project.ProjectId, files);
                Assert.IsTrue(updateProject.Success);
                Console.WriteLine("API Test: {0} Project updated successfully", source.Language);

                // Download the project again to validate its got the new file
                verifyRead = api.ReadProject(project.ProjectId);
                Assert.IsTrue(verifyRead.Files.Count == 1);
                Assert.IsTrue(verifyRead.Files.First().Name == source.Name);
                Console.WriteLine("API Test: {0} Project read back successfully", source.Language);

                // Test successfully compile the project we've created
                compileCreate = api.CreateCompile(project.ProjectId);
                Assert.IsTrue(compileCreate.Success);
                Assert.IsTrue(compileCreate.State == CompileState.InQueue);
                Console.WriteLine("API Test: {0} Compile created successfully", source.Language);

                //Read out the compile; wait for it to be completed for 10 seconds
                compileSuccess = WaitForCompilerResponse(api, project.ProjectId, compileCreate.CompileId);
                Assert.IsTrue(compileSuccess.Success);
                Assert.IsTrue(compileSuccess.State == CompileState.BuildSuccess);
                Console.WriteLine("API Test: {0} Project built successfully", source.Language);

                // Update the file, create a build error, test we get build error
                files[0].Code += "[Jibberish at end of the file to cause a build error]";
                api.UpdateProject(project.ProjectId, files);
                compileError = api.CreateCompile(project.ProjectId);
                compileError = WaitForCompilerResponse(api, project.ProjectId, compileError.CompileId);
                Assert.IsTrue(compileError.Success); // Successfully processed rest request.
                Assert.IsTrue(compileError.State == CompileState.BuildError); //Resulting in build fail.
                Console.WriteLine("API Test: {0} Project errored successfully", source.Language);

                // Using our successful compile; launch a backtest! 
                backtestName = DateTime.Now.toString("u") + " API Backtest";
                backtest = api.CreateBacktest(project.ProjectId, compileSuccess.CompileId, backtestName);
                Assert.IsTrue(backtest.Success);
                Console.WriteLine("API Test: {0} Backtest created successfully", source.Language);

                // Now read the backtest and wait for it to complete
                backtestRead = WaitForBacktestCompletion(api, project.ProjectId, backtest.BacktestId);
                Assert.IsTrue(backtestRead.Success);
                Assert.IsTrue(backtestRead.Progress == 1);
                Assert.IsTrue(backtestRead.Name == backtestName);
                Assert.IsTrue(backtestRead.Result.Statistics["Total Trades"] == "1");
                Console.WriteLine("API Test: {0} Backtest completed successfully", source.Language);

                // Verify we have the backtest in our project
                listBacktests = api.BacktestList(project.ProjectId);
                Assert.IsTrue(listBacktests.Success);
                Assert.IsTrue(listBacktests.Backtests.Count >= 1);
                Assert.IsTrue(listBacktests.Backtests[0].Name == backtestName);
                Console.WriteLine("API Test: {0} Backtests listed successfully", source.Language);

                // Update the backtest name and test its been updated
                backtestName += "-Amendment";
                renameBacktest = api.UpdateBacktest(project.ProjectId, backtest.BacktestId, backtestName);
                Assert.IsTrue(renameBacktest.Success);
                backtestRead = api.ReadBacktest(project.ProjectId, backtest.BacktestId);
                Assert.IsTrue(backtestRead.Name == backtestName);
                Console.WriteLine("API Test: {0} Backtest renamed successfully", source.Language);

                //Update the note and make sure its been updated:
                newNote = DateTime.Now.toString("u");
                noteBacktest = api.UpdateBacktest(project.ProjectId, backtest.BacktestId, backtestNote: newNote);
                Assert.IsTrue(noteBacktest.Success);
                backtestRead = api.ReadBacktest(project.ProjectId, backtest.BacktestId);
                Assert.IsTrue(backtestRead.Note == newNote);
                Console.WriteLine("API Test: {0} Backtest note added successfully", source.Language);

                // Delete the backtest we just created
                deleteBacktest = api.DeleteBacktest(project.ProjectId, backtest.BacktestId);
                Assert.IsTrue(deleteBacktest.Success);
                Console.WriteLine("API Test: {0} Backtest deleted successfully", source.Language);

                // Test delete the project we just created
                deleteProject = api.Delete(project.ProjectId);
                Assert.IsTrue(deleteProject.Success);
                Console.WriteLine("API Test: {0} Project deleted successfully", source.Language);
            }
        }


        /// <summary>
        /// Live algorithm tests
        /// </summary>
        [Test]
        public void ListAccountLiveAlgorithms()
        {
            api = CreateApiAccessor();

            // List all previously deployed algorithms
            liveAlgorithms = api.LiveList();
            Assert.IsTrue(liveAlgorithms.Success);
            Assert.IsTrue(liveAlgorithms.Algorithms.Any());
        }


        /// <summary>
        /// Create an authenticated API accessor object.
        /// </summary>
        /// <returns></returns>
        private IApi CreateApiAccessor()
        {
            return CreateApiAccessor(_testAccount, _testToken);
        }

        /// <summary>
        /// Create an API Class with the specified credentials
        /// </summary>
        /// <param name="uid">User id</param>
        /// <param name="token">Token string</param>
        /// <returns>API class for placing calls</returns>
        private IApi CreateApiAccessor(int uid, String token)
        {
            api = new Api.Api();
            api.Initialize(uid, token);
            return api;
        }

        /// <summary>
        /// Wait for the compiler to respond to a specified compile request
        /// </summary>
        /// <param name="api">API Method</param>
        /// <param name="projectId"></param>
        /// <param name="compileId"></param>
        /// <returns></returns>
        private Compile WaitForCompilerResponse(IApi api, int projectId, String compileId)
        {
            compile = new Compile();
            finish = DateTime.Now.AddSeconds(30);
            while (DateTime.Now < finish)
            {
                compile = api.ReadCompile(projectId, compileId);
                if (compile.State != CompileState.InQueue) break;
                Thread.Sleep(500);
            }
            return compile;
        }

        /// <summary>
        /// Wait for the backtest to complete
        /// </summary>
        /// <param name="api">IApi Object to make requests</param>
        /// <param name="projectId">Project id to scan</param>
        /// <param name="backtestId">Backtest id previously started</param>
        /// <returns>Completed backtest object</returns>
        private Backtest WaitForBacktestCompletion(IApi api, int projectId, String backtestId)
        {
            result = new Backtest();
            finish = DateTime.Now.AddSeconds(60);
            while (DateTime.Now < finish)
            {
                result = api.ReadBacktest(projectId, backtestId);
                if (result.Progress == 1) break;
                if (!result.Success) break;
                Thread.Sleep(500);
            }
            return result;
        }

        class TestAlgorithm
        {
            public Language Language;
            public String Code;
            public String Name;

            public TestAlgorithm(Language language, String name, String code)
            {
                Language = language;
                Code = code;
                Name = name;
            }
        }
    }

    
}
