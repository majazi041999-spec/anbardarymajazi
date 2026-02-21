let apiBase = localStorage.getItem('inventoryApiBase') || 'http://localhost:5000';

const statusEl = document.getElementById('apiStatus');
const logBox = document.getElementById('logBox');
const apiBaseInput = document.getElementById('apiBaseInput');
const receiptItemSelect = document.getElementById('receiptItemSelect');
const issueItemSelect = document.getElementById('issueItemSelect');
const supplierSelect = document.getElementById('supplierSelect');
const departmentSelect = document.getElementById('departmentSelect');

if (apiBaseInput) apiBaseInput.value = apiBase;

const setLog = (msg) => {
  logBox.textContent = `[${new Date().toLocaleTimeString('fa-IR')}] ${msg}\n` + logBox.textContent;
};

async function api(path, options = {}) {
  const res = await fetch(`${apiBase}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || 'خطا در ارتباط با سرور');
  }

  if (res.status === 204) return null;
  return res.json();
}

function fillSelect(selectEl, rows, placeholder, textFactory) {
  if (!selectEl) return;

  const currentValue = selectEl.value;
  selectEl.innerHTML = `<option value="">${placeholder}</option>`;
  rows.forEach((row) => {
    const opt = document.createElement('option');
    opt.value = textFactory ? textFactory(row).value : row.name;
    opt.textContent = textFactory ? textFactory(row).label : row.name;
    selectEl.appendChild(opt);
  });

  if (currentValue && [...selectEl.options].some((x) => x.value === currentValue)) {
    selectEl.value = currentValue;
  }
}

async function refreshItemsForForms() {
  try {
    const items = await api('/api/items');
    const mapper = (item) => ({
      value: item.id,
      label: `${item.name} (${item.code}) | موجودی: ${item.currentStock}`
    });
    fillSelect(receiptItemSelect, items, 'انتخاب کالا...', mapper);
    fillSelect(issueItemSelect, items, 'انتخاب کالا...', mapper);
  } catch (err) {
    setLog(`خطا در دریافت لیست کالاها: ${err.message}`);
  }
}

async function refreshMasterData() {
  try {
    const [suppliers, departments] = await Promise.all([
      api('/api/masters/suppliers'),
      api('/api/masters/departments')
    ]);

    fillSelect(supplierSelect, suppliers, 'انتخاب تامین‌کننده...');
    fillSelect(departmentSelect, departments, 'انتخاب واحد...');
  } catch (err) {
    setLog(`خطا در دریافت داده‌های پایه: ${err.message}`);
  }
}


async function refreshActivityTable() {
  const tbody = document.getElementById('activityTableBody');
  if (!tbody) return;

  try {
    const rows = await api('/api/dashboard/activity?take=20');
    if (!rows.length) {
      tbody.innerHTML = '<tr><td colspan="5">فعالیتی ثبت نشده است.</td></tr>';
      return;
    }

    tbody.innerHTML = rows.map((x) => {
      const badge = x.movementType === 'receipt'
        ? '<span class="badge badge-in">ورود</span>'
        : '<span class="badge badge-out">خروج</span>';
      const time = new Date(x.createdAtUtc).toLocaleString('fa-IR');
      return `<tr>
        <td>${time}</td>
        <td>${badge}</td>
        <td>${x.itemName}</td>
        <td>${x.quantity}</td>
        <td>${x.partyName}</td>
      </tr>`;
    }).join('');
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="5">خطا در دریافت فعالیت‌ها: ${err.message}</td></tr>`;
  }
}

async function refreshDashboard() {
  try {
    const [summary, lowStock] = await Promise.all([
      api('/api/dashboard/summary'),
      api('/api/items/low-stock')
    ]);

    document.getElementById('itemsCount').textContent = summary.itemsCount;
    document.getElementById('totalStock').textContent = summary.totalStock;
    document.getElementById('lowStock').textContent = summary.lowStockItemsCount;
    document.getElementById('ops7Days').textContent = summary.recentReceiptsCount + summary.recentIssuesCount;

    const list = document.getElementById('lowStockList');
    list.innerHTML = '';
    if (!lowStock.length) {
      list.innerHTML = '<li>مورد بحرانی وجود ندارد.</li>';
    } else {
      lowStock.slice(0, 10).forEach((x) => {
        const li = document.createElement('li');
        li.textContent = `${x.name} | موجودی: ${x.currentStock} | کمبود: ${x.shortage}`;
        list.appendChild(li);
      });
    }

    await Promise.all([refreshItemsForForms(), refreshMasterData(), refreshActivityTable()]);

    statusEl.textContent = `متصل به API: ${apiBase}`;
    statusEl.style.color = '#10b981';
  } catch (err) {
    statusEl.textContent = 'اتصال به API برقرار نیست (برای دموی ظاهری، UI آماده است)';
    statusEl.style.color = '#f59e0b';
    setLog(`خطای ارتباط: ${err.message}`);
  }
}

function bindApiConfigForm() {
  const form = document.getElementById('apiConfigForm');
  if (!form) return;

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    const f = new FormData(form);
    const value = String(f.get('apiBase') || '').trim();

    if (!value) {
      setLog('آدرس API نمی‌تواند خالی باشد.');
      return;
    }

    apiBase = value.replace(/\/$/, '');
    localStorage.setItem('inventoryApiBase', apiBase);
    setLog(`آدرس API ذخیره شد: ${apiBase}`);
    await refreshDashboard();
  });
}

function bindMasterButtons() {
  document.getElementById('addSupplierBtn')?.addEventListener('click', async () => {
    const input = document.getElementById('newSupplierInput');
    const name = (input?.value || '').trim();
    if (!name) return;

    try {
      await api('/api/masters/suppliers', { method: 'POST', body: JSON.stringify({ name }) });
      input.value = '';
      setLog(`تامین‌کننده جدید اضافه شد: ${name}`);
      refreshMasterData();
    } catch (err) {
      setLog(`خطا در افزودن تامین‌کننده: ${err.message}`);
    }
  });

  document.getElementById('addDepartmentBtn')?.addEventListener('click', async () => {
    const input = document.getElementById('newDepartmentInput');
    const name = (input?.value || '').trim();
    if (!name) return;

    try {
      await api('/api/masters/departments', { method: 'POST', body: JSON.stringify({ name }) });
      input.value = '';
      setLog(`واحد جدید اضافه شد: ${name}`);
      refreshMasterData();
    } catch (err) {
      setLog(`خطا در افزودن واحد: ${err.message}`);
    }
  });
}

function bindForms() {
  document.getElementById('itemForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const f = new FormData(e.target);
    const body = {
      code: f.get('code'),
      name: f.get('name'),
      unit: f.get('unit'),
      minStockLevel: Number(f.get('minStockLevel') || 0)
    };

    try {
      const result = await api('/api/items', { method: 'POST', body: JSON.stringify(body) });
      setLog(`کالا ثبت شد: ${result.name} - شناسه: ${result.id}`);
      e.target.reset();
      refreshDashboard();
    } catch (err) {
      setLog(`خطا در ثبت کالا: ${err.message}`);
    }
  });

  document.getElementById('receiptForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const f = new FormData(e.target);
    const body = {
      itemId: f.get('itemId'),
      quantity: Number(f.get('quantity') || 0),
      supplierName: f.get('supplierName'),
      referenceNo: f.get('referenceNo')
    };

    try {
      await api('/api/receipts', { method: 'POST', body: JSON.stringify(body) });
      setLog('رسید انبار با موفقیت ثبت شد.');
      e.target.reset();
      refreshDashboard();
    } catch (err) {
      setLog(`خطا در ثبت رسید: ${err.message}`);
    }
  });

  document.getElementById('issueForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const f = new FormData(e.target);
    const body = {
      itemId: f.get('itemId'),
      quantity: Number(f.get('quantity') || 0),
      departmentName: f.get('departmentName'),
      referenceNo: f.get('referenceNo')
    };

    try {
      await api('/api/issues', { method: 'POST', body: JSON.stringify(body) });
      setLog('حواله خروج با موفقیت ثبت شد.');
      e.target.reset();
      refreshDashboard();
    } catch (err) {
      setLog(`خطا در ثبت حواله: ${err.message}`);
    }
  });
}

bindApiConfigForm();
bindMasterButtons();
bindForms();
refreshDashboard();
