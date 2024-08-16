create table award_generation_groups (
    id serial not null,
        primary key (id),
    name text not null,
    started_at timestamptz not null,
    ended_at timestamptz not null,
    unique (name, started_at, ended_at)
);

create table awards (
    id serial not null,
        primary key (id),
    generation_group_id int not null
        references award_generation_groups (id),
    type int not null, -- TODO: put behind an enum?
    nominator_id int not null
        references nominators (id),
    count int not null,
    unique (generation_group_id, type, nominator_id)
);
