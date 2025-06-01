CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    firstname VARCHAR(255) NOT NULL,
    lastname VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    phonenumber VARCHAR(255) UNIQUE NOT NULL,
    password TEXT NOT NULL,
    token TEXT,
    refreshtoken TEXT,
    refreshtokenexpirytime TIMESTAMP NOT NULL
);

CREATE TABLE addresses (
    id SERIAL PRIMARY KEY,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL
);

CREATE TABLE order_statuses (
    id SERIAL PRIMARY KEY,
    order_id INT NOT NULL,
    date_in TIMESTAMP NOT NULL,
    status INT NOT NULL
);

CREATE TABLE orders (
    id SERIAL PRIMARY KEY

    sender_id INT NOT NULL,
    recipient_id INT NOT NULL,
    pickup_address_id INT NOT NULL,
    delivery_address_id INT NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    estimated_delivery TIMESTAMP WITHOUT TIME ZONE,
    completed_at TIMESTAMP WITHOUT TIME ZONE,
    cost DOUBLE PRECISION NOT NULL,
    CONSTRAINT fk_sender FOREIGN KEY (sender_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_recipient FOREIGN KEY (recipient_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_pickup FOREIGN KEY (pickup_address_id) REFERENCES addresses(id) ON DELETE CASCADE,
    CONSTRAINT fk_delivery FOREIGN KEY (delivery_address_id) REFERENCES addresses(id) ON DELETE CASCADE
);

CREATE TABLE package_details (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id INT NOT NULL,
    weight_kg DOUBLE PRECISION NOT NULL,
    width_cm DOUBLE PRECISION NOT NULL,
    height_cm DOUBLE PRECISION NOT NULL,
    depth_cm DOUBLE PRECISION NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);

CREATE TABLE robots (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    model VARCHAR(255) NOT NULL
);

CREATE TABLE robot_assignments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    robot_id UUID NOT NULL,
    order_id INT NOT NULL,
    assignment_at TIMESTAMP NOT NULL,
    waiting_time INT NOT NULL,
    FOREIGN KEY (robot_id) REFERENCES robots(id) ON DELETE CASCADE,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);

CREATE TABLE robot_specifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    robot_id UUID NOT NULL,
    load_capacity_kg DOUBLE PRECISION NOT NULL,
    width DOUBLE PRECISION NOT NULL,
    height DOUBLE PRECISION NOT NULL,
    depth DOUBLE PRECISION NOT NULL,
    max_speed_kph DOUBLE PRECISION NOT NULL,
    battery_capacity_ah DOUBLE PRECISION NOT NULL,
    energy_consumption_per_m DOUBLE PRECISION NOT NULL,
    FOREIGN KEY (robot_id) REFERENCES robots(id) ON DELETE CASCADE
);

CREATE TABLE robot_telemetries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    robot_id UUID NOT NULL,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    battery_level DOUBLE PRECISION NOT NULL,
    speed_kph DOUBLE PRECISION NOT NULL,
    last_update TIMESTAMP NOT NULL,
    status INT NOT NULL,
    FOREIGN KEY (robot_id) REFERENCES robots(id) ON DELETE CASCADE
);

CREATE TABLE nodes (
    id BIGSERIAL PRIMARY KEY,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    is_building_center BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE edges (
    id BIGSERIAL PRIMARY KEY,
    from_node_id BIGINT NOT NULL,
    to_node_id BIGINT NOT NULL,
    cost DOUBLE PRECISION NOT NULL,
    CONSTRAINT fk_edges_from_node FOREIGN KEY (from_node_id) REFERENCES nodes(id) ON DELETE CASCADE,
    CONSTRAINT fk_edges_to_node FOREIGN KEY (to_node_id) REFERENCES nodes(id) ON DELETE CASCADE
);

CREATE INDEX idx_edges_from_node_id ON edges(from_node_id);
CREATE INDEX idx_edges_to_node_id ON edges(to_node_id);

CREATE TABLE routes (
    id BIGSERIAL PRIMARY KEY,
    start_node_id BIGINT NOT NULL,
    end_node_id BIGINT NOT NULL,
    total_distance DOUBLE PRECISION NOT NULL,
    create_at TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    route_type INT NOT NULL,
    order_id INT NOT NULL,

    CONSTRAINT fk_route_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    CONSTRAINT fk_start_node FOREIGN KEY (start_node_id) REFERENCES nodes(id) ON DELETE CASCADE,
    CONSTRAINT fk_end_node FOREIGN KEY (end_node_id) REFERENCES nodes(id) ON DELETE CASCADE
);

CREATE TABLE route_nodes (
    id BIGSERIAL PRIMARY KEY,
    route_id BIGINT NOT NULL,
    node_id BIGINT NOT NULL,

    CONSTRAINT fk_route_node_route FOREIGN KEY (route_id) REFERENCES routes(id) ON DELETE CASCADE,
    CONSTRAINT fk_route_node_node FOREIGN KEY (node_id) REFERENCES nodes(id) ON DELETE CASCADE
)
