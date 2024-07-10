alter table projects
    add column created_at timestamptz not null
        default (now() at time zone 'UTC'),
    add column updated_at timestamptz not null
        default (now() at time zone 'UTC');
