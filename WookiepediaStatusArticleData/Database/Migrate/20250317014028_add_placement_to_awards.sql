-- 3 corresponds to the "DidNotPlace" enum variant, which is the default we'll use
alter table awards add column if not exists placement int not null default 3;