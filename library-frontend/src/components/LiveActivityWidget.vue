<template>
  <div class="widget">
    <div class="widget-top">
      <div class="widget-title">
        <span :class="['status-dot', connectionStatus]"></span>
        <h2>Live Activity</h2>
      </div>
    </div>

    <div v-if="events.length" class="widget-feed">
      <div
        v-for="event in events"
        :key="event.key"
        :class="['widget-item', event.action === 'Borrowed' ? 'borrowed' : 'returned']"
      >
        <span class="widget-icon">
          {{ event.action === "Borrowed" ? "📕" : "📗" }}
        </span>

        <span class="widget-text">
          <strong>{{ event.bookTitle }}</strong>
          {{ event.action === "Borrowed" ? "borrowed" : "returned" }}
        </span>

        <span class="widget-copies">
          {{ event.availableCopies }}/{{ event.totalCopies }}
        </span>
      </div>
    </div>

    <p v-else class="widget-empty">
      No activity yet. Borrow/return activity will appear here live.
    </p>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from "vue";
import * as signalR from "@microsoft/signalr";

// Home sayfasında gösterilen kompakt, canlı kitap durumu akışı.
// Kendi SignalR bağlantısını kurar ve sadece son birkaç event'i gösterir.
const MAX_WIDGET_EVENTS = 4;
const API_BASE_URL = "http://localhost:5239";

const events = ref([]);
const connectionStatus = ref("connecting");
let connection = null;
let eventCounter = 0;

const startConnection = async () => {
  const token = localStorage.getItem("token");

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/loan`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .build();

  connection.on("BookStatusChanged", (data) => {
    eventCounter += 1;
    events.value.unshift({ ...data, key: eventCounter });
    events.value = events.value.slice(0, MAX_WIDGET_EVENTS);
  });

  connection.onreconnecting(() => {
    connectionStatus.value = "connecting";
  });

  connection.onreconnected(() => {
    connectionStatus.value = "connected";
  });

  connection.onclose(() => {
    connectionStatus.value = "disconnected";
  });

  try {
    await connection.start();
    connectionStatus.value = "connected";
  } catch (error) {
    console.error("SignalR connection failed:", error);
    connectionStatus.value = "disconnected";
  }
};

onMounted(startConnection);

onBeforeUnmount(() => {
  if (connection) {
    connection.stop();
  }
});
</script>

<style scoped>
.widget {
  border-radius: 22px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
  padding: 20px 22px;
  margin-bottom: 22px;
}

.widget-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
}

.widget-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.widget-title h2 {
  margin: 0;
  font-size: 18px;
  color: #0f172a;
}

.status-dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: #f59e0b;
}

.status-dot.connected {
  background: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.18);
}

.status-dot.connecting {
  background: #f59e0b;
  box-shadow: 0 0 0 3px rgba(245, 158, 11, 0.18);
}

.status-dot.disconnected {
  background: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.18);
}

.widget-feed {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.widget-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 13px;
  background: #f8fafc;
  font-size: 13px;
}

.widget-icon {
  font-size: 16px;
}

.widget-text {
  flex: 1;
  color: #334155;
  font-weight: 700;
}

.widget-item.borrowed .widget-text strong {
  color: #991b1b;
}

.widget-item.returned .widget-text strong {
  color: #166534;
}

.widget-copies {
  color: #64748b;
  font-weight: 800;
  font-size: 12px;
}

.widget-empty {
  margin: 0;
  padding: 10px 2px;
  color: #64748b;
  font-weight: 700;
  font-size: 14px;
}
</style>