create table historical_projects (
    id serial not null,
    project_id int not null references projects (id),
    action_type text not null check (action_type in ('Create', 'Update', 'Archive')),
    name text not null,
    occurred_at timestamptz not null
);

create index historical_projects_project_id_occurred_at on historical_projects (project_id, occurred_at);

insert into historical_projects (project_id, action_type, name, occurred_at)
select id, 'Create', name, created_at from projects;

alter table projects
    add column is_archived bool not null default false;
