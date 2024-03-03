alter table projects
    add type text not null
        default 'Category'
        check (type in ('Category', 'IntellectualProperty'));

alter table historical_projects
    add type text not null
        default 'Category'
        check (type in ('Category', 'IntellectualProperty'));
