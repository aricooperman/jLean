package com.quantconnect.lean.event;

public class CollectionChangedEvent {
    
    public enum CollectionChangedAction {
        Add, Remove
        
    }

    private final CollectionChangedAction action;
    private final Object element;

    public CollectionChangedEvent( CollectionChangedAction action, Object element ) {
        this.action = action;
        this.element = element;
    }

    public CollectionChangedAction getAction() {
        return action;
    }

    public Object getElement() {
        return element;
    }

}
