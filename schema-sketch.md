
# ERP Schema Sketch (PostgreSQL · Per-Service Schemas)

> **Purpose:** A starting blueprint for tables, constraints, and emitted events per microservice.  
> **Rule:** Each microservice owns **its own schema** (no cross-service FKs). Exchange data via REST/events.

---

## Global Conventions

- **IDs:** `uuid` (consider UUID v7 for index locality).  
- **Timestamps:** `timestamptz` (`created_at`, `updated_at` where useful).  
- **Money:** `numeric(18,2)` with `currency char(3)` (ISO-4217).  
- **JSON:** use `jsonb` for flexible attributes or payloads.  
- **Optimistic concurrency:** `xmin` or an explicit `row_version bigint`.  
- **Outbox:** each service schema contains `_outbox` for reliable pub.  
- **Indexes:** include practical examples; refine after profiling.  
- **No cross-service FK constraints:** use GUID references + validation via service APIs/events.

---

## Catalog Schema (`catalog`)

### Purpose
Owns product master data and category taxonomy.

### Tables
- **category**  
  `id uuid PK, name text not null, parent_id uuid null` (self-reference)

- **product**  
  `id uuid PK, sku text unique, name text, description text, category_id uuid?, image_main_url text?, attributes jsonb?, is_active boolean default true, created_at timestamptz`

### Example DDL
```sql
create schema if not exists catalog;

create table catalog.category(
  id uuid primary key,
  name text not null,
  parent_id uuid null
);

create table catalog.product(
  id uuid primary key,
  sku text not null,
  name text not null,
  description text null,
  category_id uuid null,
  image_main_url text null,
  attributes jsonb null,
  is_active boolean not null default true,
  created_at timestamptz not null default now()
);

create unique index ux_product_sku on catalog.product (sku);
create index ix_product_category on catalog.product (category_id);
create index ix_product_active on catalog.product (is_active);

-- Optional intra-service FK
alter table catalog.product
  add constraint fk_product_category
  foreign key (category_id) references catalog.category(id);
```

### Emits
- `ProductCreated`
- `ProductPriceChanged` *(or move to Pricing service if split)*

---

## Procurement Schema (`procurement`)

### Purpose
Manages suppliers, supplier price lists, purchase orders, and goods receipts.

### Tables
- **supplier**  
  `id uuid PK, name text, contact jsonb?, tax_id text?, is_active boolean default true, created_at timestamptz`

- **supplier_product**  
  `(supplier_id uuid, product_id uuid, supplier_sku text?, price numeric(18,2), currency char(3), lead_time_days int?, PRIMARY KEY(supplier_id, product_id))`

- **purchase_order**  
  `id uuid PK, supplier_id uuid, status text check (...), created_at timestamptz, expected_at date?`

- **purchase_order_line**  
  `id uuid PK, purchase_order_id uuid, product_id uuid, qty int > 0, unit_price numeric(18,2)`

- **goods_receipt**  
  `id uuid PK, purchase_order_id uuid, received_at timestamptz`

- **goods_receipt_line**  
  `id uuid PK, goods_receipt_id uuid, product_id uuid, qty_received int >= 0`

### Example DDL
```sql
create schema if not exists procurement;

create table procurement.supplier(
  id uuid primary key,
  name text not null,
  contact jsonb null,
  tax_id text null,
  is_active boolean not null default true,
  created_at timestamptz not null default now()
);

create table procurement.supplier_product(
  supplier_id uuid not null,
  product_id uuid not null, -- Catalog product (no cross-service FK)
  supplier_sku text null,
  price numeric(18,2) not null,
  currency char(3) not null,
  lead_time_days int null,
  primary key (supplier_id, product_id)
);

create table procurement.purchase_order(
  id uuid primary key,
  supplier_id uuid not null,
  status text not null check (status in ('Draft','Submitted','Partial','Received','Cancelled')),
  created_at timestamptz not null default now(),
  expected_at date null
);

create table procurement.purchase_order_line(
  id uuid primary key,
  purchase_order_id uuid not null,
  product_id uuid not null, -- Catalog product
  qty int not null check (qty > 0),
  unit_price numeric(18,2) not null
);

create table procurement.goods_receipt(
  id uuid primary key,
  purchase_order_id uuid not null,
  received_at timestamptz not null default now()
);

create table procurement.goods_receipt_line(
  id uuid primary key,
  goods_receipt_id uuid not null,
  product_id uuid not null,
  qty_received int not null check (qty_received >= 0)
);

create index ix_po_supplier on procurement.purchase_order (supplier_id, status);
create index ix_pol_po on procurement.purchase_order_line (purchase_order_id);
create index ix_gr_po on procurement.goods_receipt (purchase_order_id);
create index ix_grl_gr on procurement.goods_receipt_line (goods_receipt_id);
```

### Emits
- `PurchaseOrderSubmitted`
- `GoodsReceived`

---

## Inventory Schema (`inventory`)

### Purpose
Tracks warehouses, stock levels, movements, and reservations.

### Tables
- **warehouse**  
  `id uuid PK, code text unique, name text, address jsonb?, created_at timestamptz`

- **location** *(optional bins)*  
  `id uuid PK, warehouse_id uuid, code text, UNIQUE(warehouse_id, code)`

- **stock_item**  
  `id uuid PK, product_id uuid`

- **stock_level**  
  `stock_item_id uuid, warehouse_id uuid, on_hand int default 0, reserved int default 0, updated_at timestamptz, PRIMARY KEY(stock_item_id, warehouse_id)`

- **stock_movement**  
  `id uuid PK, stock_item_id uuid, warehouse_id uuid, qty int, type text check ('IN','OUT','ADJUST'), ref_type text?, ref_id uuid?, at timestamptz`

- **reservation**  
  `id uuid PK, stock_item_id uuid, warehouse_id uuid, order_id uuid, qty int > 0, status text check (...), created_at timestamptz`

### Example DDL
```sql
create schema if not exists inventory;

create table inventory.warehouse(
  id uuid primary key,
  code text not null,
  name text not null,
  address jsonb null,
  created_at timestamptz not null default now(),
  unique(code)
);

create table inventory.location(
  id uuid primary key,
  warehouse_id uuid not null,
  code text not null,
  unique(warehouse_id, code)
);

create table inventory.stock_item(
  id uuid primary key,
  product_id uuid not null -- Catalog product (no cross-service FK)
);

create table inventory.stock_level(
  stock_item_id uuid not null,
  warehouse_id uuid not null,
  on_hand int not null default 0,
  reserved int not null default 0,
  updated_at timestamptz not null default now(),
  primary key (stock_item_id, warehouse_id)
);

create table inventory.stock_movement(
  id uuid primary key,
  stock_item_id uuid not null,
  warehouse_id uuid not null,
  qty int not null,
  type text not null check (type in ('IN','OUT','ADJUST')),
  ref_type text null,
  ref_id uuid null,
  at timestamptz not null default now()
);

create table inventory.reservation(
  id uuid primary key,
  stock_item_id uuid not null,
  warehouse_id uuid not null,
  order_id uuid not null, -- Sales order id
  qty int not null check (qty > 0),
  status text not null check (status in ('Reserved','Released','Consumed','Shortage')),
  created_at timestamptz not null default now()
);

create index ix_level_item_wh on inventory.stock_level (stock_item_id, warehouse_id);
create index ix_move_item_time on inventory.stock_movement (stock_item_id, at);
create index ix_res_item_wh on inventory.reservation (stock_item_id, warehouse_id, status);
```

### Notes
- `available = on_hand - reserved`. Maintain `stock_level` as source of truth.
- Movements are append-only; use for audit/history.
- Consider monthly partitioning on `stock_movement` at scale.

### Emits
- `InventoryAdjusted`
- `StockReserved`
- `StockShortage`
- `StockReleased`

---

## Sales Schema (`sales`)

### Purpose
Captures orders, order lines, and payments (or delegate payments to a dedicated service).

### Tables
- **sales_order**  
  `id uuid PK, channel text check ('POS','ECOM','API'), customer_id uuid?, contractor_id uuid?, status text check (...), placed_at timestamptz`

- **sales_order_line**  
  `id uuid PK, sales_order_id uuid, product_id uuid, qty int > 0, unit_price numeric(18,2), currency char(3)`

- **payment** *(optional here; often a separate service)*  
  `id uuid PK, sales_order_id uuid, provider text, status text check (...), amount numeric(18,2), currency char(3), txn_ref text?`

### Example DDL
```sql
create schema if not exists sales;

create table sales.sales_order(
  id uuid primary key,
  channel text not null check (channel in ('POS','ECOM','API')),
  customer_id uuid null,
  contractor_id uuid null,
  status text not null check (status in ('Pending','AwaitingStock','AwaitingPayment','Paid','Picking','Shipped','Delivered','Cancelled')),
  placed_at timestamptz not null default now()
);

create table sales.sales_order_line(
  id uuid primary key,
  sales_order_id uuid not null,
  product_id uuid not null,
  qty int not null check (qty > 0),
  unit_price numeric(18,2) not null,
  currency char(3) not null
);

create table sales.payment(
  id uuid primary key,
  sales_order_id uuid not null,
  provider text not null,
  status text not null check (status in ('Pending','Authorized','Captured','Failed','Refunded','Cancelled')),
  amount numeric(18,2) not null,
  currency char(3) not null,
  txn_ref text null
);

create index ix_order_status on sales.sales_order (status, placed_at);
create index ix_line_order on sales.sales_order_line (sales_order_id);
create index ix_payment_order on sales.payment (sales_order_id);
```

### Emits
- `SalesOrderCreated`
- `SalesOrderPaid`
- `SalesOrderCancelled`

---

## Fulfillment Schema (`fulfillment`)

### Purpose
Handles pick/pack/ship and shipment tracking.

### Tables
- **picklist**  
  `id uuid PK, warehouse_id uuid, sales_order_id uuid, status text check (...)`

- **picklist_line**  
  `id uuid PK, picklist_id uuid, product_id uuid, qty int > 0`

- **shipment**  
  `id uuid PK, sales_order_id uuid, carrier text?, tracking_no text?, status text check (...), shipped_at timestamptz?, delivered_at timestamptz?`

### Example DDL
```sql
create schema if not exists fulfillment;

create table fulfillment.picklist(
  id uuid primary key,
  warehouse_id uuid not null,
  sales_order_id uuid not null,
  status text not null check (status in ('Pending','Picking','Packed','Complete','Cancelled'))
);

create table fulfillment.picklist_line(
  id uuid primary key,
  picklist_id uuid not null,
  product_id uuid not null,
  qty int not null check (qty > 0)
);

create table fulfillment.shipment(
  id uuid primary key,
  sales_order_id uuid not null,
  carrier text null,
  tracking_no text null,
  status text not null check (status in ('Pending','Shipped','Delivered','Cancelled','ReturnRequested','Returned')),
  shipped_at timestamptz null,
  delivered_at timestamptz null
);

create index ix_picklist_order on fulfillment.picklist (sales_order_id);
create index ix_ship_order on fulfillment.shipment (sales_order_id, status);
```

### Emits
- `ShipmentCreated`
- `ShipmentDelivered`

---

## Contractors & Jobs Schema (`contracting`) — (later)

### Purpose
Manage contractors, installers, leads, surveys, and quotes; convert accepted quotes to sales orders.

### Tables
- **contractor**  
  `id uuid PK, org_name text, vat text?, billing_address jsonb?`

- **installer**  
  `id uuid PK, contractor_id uuid, name text, contact jsonb?`

- **lead**  
  `id uuid PK, contractor_id uuid, address jsonb?, source text?, status text check (...)`

- **survey**  
  `id uuid PK, lead_id uuid, assessor_id uuid?, results jsonb?, surveyed_at timestamptz?`

- **quote**  
  `id uuid PK, lead_id uuid, status text check (...), total numeric(18,2), currency char(3), created_at timestamptz`

- **quote_line**  
  `id uuid PK, quote_id uuid, product_id uuid, qty int > 0, price numeric(18,2)`

### Example DDL
```sql
create schema if not exists contracting;

create table contracting.contractor(
  id uuid primary key,
  org_name text not null,
  vat text null,
  billing_address jsonb null
);

create table contracting.installer(
  id uuid primary key,
  contractor_id uuid not null,
  name text not null,
  contact jsonb null
);

create table contracting.lead(
  id uuid primary key,
  contractor_id uuid not null,
  address jsonb null,
  source text null,
  status text not null check (status in ('New','Qualified','Quoted','Won','Lost'))
);

create table contracting.survey(
  id uuid primary key,
  lead_id uuid not null,
  assessor_id uuid null,
  results jsonb null,
  surveyed_at timestamptz null
);

create table contracting.quote(
  id uuid primary key,
  lead_id uuid not null,
  status text not null check (status in ('Draft','Proposed','Accepted','Rejected','Expired')),
  total numeric(18,2) not null default 0,
  currency char(3) not null,
  created_at timestamptz not null default now()
);

create table contracting.quote_line(
  id uuid primary key,
  quote_id uuid not null,
  product_id uuid not null,
  qty int not null check (qty > 0),
  price numeric(18,2) not null
);

create index ix_installer_contractor on contracting.installer (contractor_id);
create index ix_lead_contractor on contracting.lead (contractor_id, status);
create index ix_quote_lead on contracting.quote (lead_id, status);
create index ix_ql_quote on contracting.quote_line (quote_id);
```

### Emits
- `QuoteAccepted`
- `WorkOrderCreated`

> **Flow:** Accepted quotes create **Sales Orders** that must be fulfilled from **Inventory**.

---

## Inter‑Service Communication

- **Sync**: Minimal REST; consider gRPC internally if you need higher performance contracts.
- **Async**: RabbitMQ topics/queues; events are versioned (`*.v1`) and schema’d (AsyncAPI/JSON Schema).
- **Reliability**: Use the **Outbox pattern** in each schema:
```sql
create table <schema>._outbox(
  id uuid primary key,
  occurred_at timestamptz not null default now(),
  type text not null,
  payload jsonb not null,
  processed_at timestamptz null
);
create index ix_outbox_unprocessed on <schema>._outbox (processed_at) where processed_at is null;
```

---

## Notes & Next Steps

- Add auditing (`created_by`, `updated_by`) if needed.  
- Consider partitioning for `stock_movement` and `_outbox` at scale.  
- Keep this doc synchronized with EF Core migrations (source of truth).  
- Add per-service READ models for aggregated UI needs; do not cross-join services at query time.
