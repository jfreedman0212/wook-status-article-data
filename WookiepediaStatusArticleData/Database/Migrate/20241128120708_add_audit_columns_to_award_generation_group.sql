alter table award_generation_groups
    add column if not exists created_at timestamptz not null default (now() at time zone 'utc'),
    add column if not exists updated_at timestamptz not null default (now() at time zone 'utc');