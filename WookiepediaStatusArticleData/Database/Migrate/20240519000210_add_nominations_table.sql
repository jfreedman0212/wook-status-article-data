create table nominations (
    id serial not null,
        primary key (id),
    article_name text not null,
    continuities int not null,
    type text not null,
    outcome text not null,
    started_at timestamptz,
    ended_at timestamptz,
    start_word_count int,
    end_word_count int
);

create table nomination_nominators (
    id serial not null,
        primary key (id),
    nomination_id int not null
        references nominations (id),
    nominator_id int not null
        references nominators (id),
    unique (nomination_id, nominator_id)
);

create table nomination_projects (
    id serial not null,
        primary key (id),
    nomination_id int not null
        references nominations (id),
    project_id int not null
        references projects (id),
    unique (nomination_id, project_id)
);
