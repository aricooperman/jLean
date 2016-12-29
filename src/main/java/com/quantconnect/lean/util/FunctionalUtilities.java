package com.quantconnect.lean.util;

import java.util.Iterator;
import java.util.Objects;
import java.util.Spliterator;
import java.util.Spliterators;
import java.util.function.BiFunction;
import java.util.stream.Stream;
import java.util.stream.StreamSupport;

public class FunctionalUtilities {

    /*
     * Taken with much appreciation from http://stackoverflow.com/questions/17640754/zipping-streams-using-jdk8-with-lambda-java-util-stream-streams-zip
     */
    public static<A, B, C> Stream<C> zip( Stream<? extends A> a, Stream<? extends B> b, BiFunction<? super A, ? super B, ? extends C> zipper ) {
        Objects.requireNonNull(zipper);
        final Spliterator<? extends A> aSpliterator = Objects.requireNonNull( a ).spliterator();
        final Spliterator<? extends B> bSpliterator = Objects.requireNonNull( b ).spliterator();
        
        // Zipping looses DISTINCT and SORTED characteristics
        int characteristics = aSpliterator.characteristics() & bSpliterator.characteristics() & ~(Spliterator.DISTINCT | Spliterator.SORTED);
        
        final long zipSize = ((characteristics & Spliterator.SIZED) != 0) ? Math.min( aSpliterator.getExactSizeIfKnown(), bSpliterator.getExactSizeIfKnown() ) : -1;
        
        final Iterator<A> aIterator = Spliterators.iterator( aSpliterator );
        final Iterator<B> bIterator = Spliterators.iterator( bSpliterator );
        final Iterator<C> cIterator = new Iterator<C>() {
            @Override
            public boolean hasNext() {
                return aIterator.hasNext() && bIterator.hasNext();
            }
            
            @Override
            public C next() {
                return zipper.apply( aIterator.next(), bIterator.next() );
            }
        };
        
        final Spliterator<C> split = Spliterators.spliterator( cIterator, zipSize, characteristics );
        return (a.isParallel() || b.isParallel()) ? StreamSupport.stream( split, true ) : StreamSupport.stream( split, false );
    }
}
