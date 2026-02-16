alter table awards
    add column if not exists code varchar(255) not null default 'PLACEHOLDER';
