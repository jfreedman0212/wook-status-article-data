create table projects (
    id serial not null,
        primary key (id),
    name text not null unique
);
