package com.quantconnect.lean.orders;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.OptionalInt;

import com.quantconnect.lean.Extensions;

/**
 * Defines a request to update an order's values
 */
public class UpdateOrderRequest extends OrderRequest {
    
    private OptionalInt quantity;
    private Optional<BigDecimal> limitPrice;
    private Optional<BigDecimal> stopPrice;
    
    /**
     * Gets <see cref="Orders.OrderRequestType.Update"/>
     */
    @Override
    public OrderRequestType getOrderRequestType() {
        return OrderRequestType.Update;
    }
    
    /**
     * Gets the new quantity of the order, null to not change the quantity
     */
    public OptionalInt getQuantity() {
        return quantity;
    }

    /**
     * Gets the new limit price of the order, null to not change the limit price
     */
    public Optional<BigDecimal> getLimitPrice() {
        return limitPrice;
    }

    /**
     * Gets the new stop price of the order, null to not change the stop price
     */
    public Optional<BigDecimal> getStopPrice() {
        return stopPrice;
    }

    /**
     * Initializes a new instance of the <see cref="UpdateOrderRequest"/> class
     * @param time The time the request was submitted
     * @param orderId The order id to be updated
     * @param fields The fields defining what should be updated
    */
    public UpdateOrderRequest( LocalDateTime time, int orderId, UpdateOrderFields fields ) {
        super( time, orderId, fields.getTag() );
        this.quantity = fields.getQuantity();
        this.limitPrice = fields.getLimitPrice();
        this.stopPrice = fields.getStopPrice();
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
    */
    @Override
    public String toString() {
        List<String> updates = new ArrayList<String>();
        
        if( quantity.isPresent() )
            updates.add( "Quantity: " + quantity.getAsInt() );

        if( limitPrice.isPresent() )
            updates.add( "LimitPrice: " + Extensions.smartRounding( limitPrice.get() ) );
        
        if( stopPrice.isPresent() )
            updates.add( "StopPrice: " + Extensions.smartRounding( stopPrice.get() ) );
        
        return String.format( "%1$s UTC: Update Order: (%2$s) - %3$s %4$s Status: %5$s", getTime(), orderId, String.join( ", ", updates ), getTag(), getStatus() );
    }
}
