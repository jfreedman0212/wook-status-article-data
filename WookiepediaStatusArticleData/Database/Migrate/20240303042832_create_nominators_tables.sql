create table nominators (
    id serial not null,
        primary key (id),
    name text not null unique
);

create table nominator_attributes (
    id serial not null,
        primary key (id),
    nominator_id int not null references nominators (id),
    attribute_name text not null,
    effective_at timestamptz not null,
    effective_end_at timestamptz
);
