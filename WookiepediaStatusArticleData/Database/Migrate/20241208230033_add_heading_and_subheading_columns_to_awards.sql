-- new heading and subheading fields act as "prefixes" for the type

alter table awards
    add column if not exists heading text not null default 'Default Heading',
    add column if not exists subheading text not null default 'Default Subheading',
    drop constraint if exists awards_generation_group_id_type_nominator_id_key;

drop index if exists awards_generation_group_id_type_nominator_id_key;

create unique index on awards (
    generation_group_id,
    heading,
    subheading,
    type,
    nominator_id
);
