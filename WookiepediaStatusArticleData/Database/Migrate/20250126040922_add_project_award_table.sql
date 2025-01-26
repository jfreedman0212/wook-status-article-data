create table project_awards (
    id serial not null,
        primary key (id),
    generation_group_id int not null
        references award_generation_groups (id),
    heading text not null,
    type text not null,
    project_id int not null
        references projects (id),
    count int not null,
    unique (generation_group_id, heading, type, project_id)
);
