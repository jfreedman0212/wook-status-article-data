-- Create award_templates table for configurable award definitions
create table award_templates (
    id serial not null,
        primary key (id),
    name text not null,
    description text not null,
    heading text not null,
    subheading text not null,
    type text not null,
    count_mode int not null,
    is_active boolean not null default true,
    sort_order int not null default 0,
    created_at timestamptz not null default (now() at time zone 'utc'),
    updated_at timestamptz not null default (now() at time zone 'utc'),
    unique (name)
);

-- Create award_criteria table for defining filtering criteria for awards
create table award_criteria (
    id serial not null,
        primary key (id),
    award_template_id int not null
        references award_templates (id) on delete cascade,
    nomination_type text null,
    continuity int null,
    panelist_filter int null,
    project_id int null
        references projects (id),
    project_filter int null,
    created_at timestamptz not null default (now() at time zone 'utc'),
    updated_at timestamptz not null default (now() at time zone 'utc')
);

-- Create index for better performance on award template lookups
create index idx_award_templates_active_sort on award_templates (is_active, sort_order);

-- Create index for award criteria foreign key
create index idx_award_criteria_template_id on award_criteria (award_template_id);

-- Create index for project lookups in criteria
create index idx_award_criteria_project_id on award_criteria (project_id) where project_id is not null;