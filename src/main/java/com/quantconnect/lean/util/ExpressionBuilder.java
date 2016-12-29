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

package com.quantconnect.lean.util;

import java.lang.reflect.Field;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.function.Function;

/**
 * Provides methods for constructing expressions at runtime
 */
public class ExpressionBuilder {
    
    private ExpressionBuilder() { }
    
    /**
     * Constructs a selector of the form: x -> x.propertyOrField where x is an instance of 'type'
     * @param type The type of the parameter in the expression
     * @param propertyOrField The name of the property or field to bind to
     * @returns A new lambda expression that represents accessing the property or field on 'type'
     */
    @SuppressWarnings("unchecked")
    public static <T, TProperty> Function<T,TProperty> makePropertyOrFieldSelector( Class<?extends T> type, String propertyOrField ) {
        try {
            final Method accessor = type.getMethod( propertyOrField, new Class[0] );
            return (Function<T, TProperty>)(t -> {accessor.setAccessible( true ); try {
                return (TProperty)accessor.invoke( t, new Object[0] );
            }
            catch( IllegalAccessException | IllegalArgumentException | InvocationTargetException e ) {
                throw new RuntimeException( e );
            } });
        }
        catch( Throwable e ) {
            try {
                final Field field = type.getDeclaredField( propertyOrField );
                return (Function<T, TProperty>)(t -> {field.setAccessible( true ); try {
                    return (TProperty)field.get( t );
                }
                catch( IllegalArgumentException | IllegalAccessException e1 ) {
                    throw new RuntimeException( e1 );
                } });
            }
            catch( NoSuchFieldException | SecurityException e1 ) {
                throw new RuntimeException( e1 );
            }
        }
    }

//    /**
//     * Converts the specified expression into an enumerable of expressions by walking the expression tree
//     * @param expression The expression to enumerate
//     * @returns An enumerable containing all expressions in the input expression
//     */
//    public static IEnumerable<Expression> AsEnumerable( this Expression expression ) {
//        walker = new ExpressionWalker();
//        walker.Visit(expression);
//        return walker.Expressions;
//    }
//
//    /**
//     * Returns all the expressions of the specified type in the given expression tree
//     * <typeparam name="T The type of expression to search for</typeparam>
//     * @param expression The expression to search
//     * @returns All expressions of the given type in the specified expression
//     */
//    public static IEnumerable<T> OfType<T>(this Expression expression)
//        where T : Expression
//    {
//        return expression.AsEnumerable().OfType<T>();
//    }
//
//    private class ExpressionWalker implements ExpressionVisitor {
//        public final HashSet<Expression> Expressions = new HashSet<Expression>(); 
//        public @Override Expression Visit(Expression node) {
//            Expressions.Add(node);
//            return base.Visit(node);
//        }
//    }
}
