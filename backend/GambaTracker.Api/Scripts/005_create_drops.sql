CREATE TABLE drops (
    id UUID PRIMARY KEY,
    item_name TEXT NOT NULL,
    quantity INT NOT NULL,
    event_id UUID NOT NULL REFERENCES events(id) ON DELETE CASCADE
);