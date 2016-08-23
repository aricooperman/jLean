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
using System.Linq;
using System.Linq.Expressions;

package com.quantconnect.lean.Util
{
    /**
    /// Provides methods for constructing expressions at runtime
    */
    public static class ExpressionBuilder
    {
        /**
        /// Constructs a selector of the form: x -> x.propertyOrField where x is an instance of 'type'
        */
         * @param type">The type of the parameter in the expression
         * @param propertyOrField">The name of the property or field to bind to
        @returns A new lambda expression that represents accessing the property or field on 'type'
        public static LambdaExpression MakePropertyOrFieldSelector(Type type, String propertyOrField) {
            parameter = Expression.Parameter(type);
            property = Expression.PropertyOrField(parameter, propertyOrField);
            lambda = Expression.Lambda(property, parameter);
            return lambda;
        }

        /**
        /// Constructs a selector of the form: x -> x.propertyOrField where x is an instance of 'type'
        */
        /// <typeparam name="T">The type of the parameter in the expression</typeparam>
        /// <typeparam name="TProperty">The type of the property or field being accessed in the expression</typeparam>
         * @param propertyOrField">The name of the property or field to bind to
        @returns A new lambda expression that represents accessing the property or field on 'type'
        public static Expression<Func<T, TProperty>> MakePropertyOrFieldSelector<T, TProperty>( String propertyOrField) {
            return (Expression<Func<T, TProperty>>) MakePropertyOrFieldSelector(typeof (T), propertyOrField);
        }

        /**
        /// Converts the specified expression into an enumerable of expressions by walking the expression tree
        */
         * @param expression">The expression to enumerate
        @returns An enumerable containing all expressions in the input expression
        public static IEnumerable<Expression> AsEnumerable(this Expression expression) {
            walker = new ExpressionWalker();
            walker.Visit(expression);
            return walker.Expressions;
        }

        /**
        /// Returns all the expressions of the specified type in the given expression tree
        */
        /// <typeparam name="T">The type of expression to search for</typeparam>
         * @param expression">The expression to search
        @returns All expressions of the given type in the specified expression
        public static IEnumerable<T> OfType<T>(this Expression expression)
            where T : Expression
        {
            return expression.AsEnumerable().OfType<T>();
        }

        private class ExpressionWalker : ExpressionVisitor
        {
            public final HashSet<Expression> Expressions = new HashSet<Expression>(); 
            public @Override Expression Visit(Expression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }
        }
    }
}
