alter table nominations
    alter column started_at set not null;

-- so we can order by started_at in the UI and use keyset pagination to implement infinite scroll
create index nomination_started_at on nominations (started_at);
