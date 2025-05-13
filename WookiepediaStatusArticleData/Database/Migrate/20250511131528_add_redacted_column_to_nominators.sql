alter table nominators
    add column if not exists is_redacted bool not null default false;