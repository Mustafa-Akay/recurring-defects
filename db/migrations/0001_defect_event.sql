create extension if not exists vector;

-- Tek bir arıza olayı. Kaynaktan bağımsız standart biçim (ATA Spec 2000 Logbook kaydından esinlenir).
-- Bugün tek kaynak FAA SDR; yarın bir havayolunun kendi teknik kayıtları aynı tabloya girebilir.
create table defect_event (
    id                    bigserial primary key,
    source                text        not null,                 -- 'FAA_SDR'
    source_ref            text        not null,                 -- kaynaktaki kayıt no (OperatorControlNumber)
    difficulty_date       date        not null,
    operator_code         text,
    aircraft_make         text,
    aircraft_model        text,
    registration          text,                                 -- tescil (N-numarası); uçağı tanımlar, kişiyi değil
    aircraft_total_hours  numeric,
    aircraft_total_cycles integer,
    jasc_code             char(4)     not null,                 -- metin: '0510' baştaki sıfırını korur
    ata_chapter           char(2)     generated always as (left(jasc_code, 2)::char(2)) stored,
    part_name             text,
    part_number           text,
    part_condition        text,
    part_location         text,
    part_total_hours      numeric,
    nature_of_condition   text,
    stage_of_operation    text,
    how_discovered        text,
    discrepancy_raw       text        not null,                 -- teknisyenin yazdığı ham metin
    discrepancy_clean     text,                                 -- Görev 5 doldurur
    embedding             vector(384),                          -- Faz 2 doldurur
    ingested_at           timestamptz not null default now(),

    -- Aynı kaynaktan aynı kayıt iki kez girmesin: içe alma tekrar çalıştırılabilir (idempotent) olur.
    constraint uq_defect_event_source unique (source, source_ref)
);

-- "Bu uçak tipinde, bu sistemde, bu tarih aralığında" sorguları (filo seviyesi tekrar)
create index ix_defect_event_model_jasc_date
    on defect_event (aircraft_model, jasc_code, difficulty_date);

-- "Bu uçakta, bu sistemde, son 30 günde" sorguları (aynı uçakta tekrar)
create index ix_defect_event_reg_jasc_date
    on defect_event (registration, jasc_code, difficulty_date);
