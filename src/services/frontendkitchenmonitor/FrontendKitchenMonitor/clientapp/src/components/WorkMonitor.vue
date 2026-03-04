<script setup>
import { computed, onMounted } from 'vue'
import { useKitchenStore } from '@/stores/kitchenStore'

const ks = useKitchenStore()

onMounted(async () => {
  await ks.fetchPendingOrders()
  await ks.initializeSignalRHub()
})

// Presentational projection with items sorted: AwaitingPreparation first, InPreparation second, Finished last
const pendingOrders = computed(() => ks.pendingOrders.map(x => {
  const items = (x.items?.map(y => ({ id: y.id, name: y.productDescription, quantity: y.quantity, state: y.state })) ?? [])
    .slice()
    .sort((a, b) => {
      const stateOrder = { 'AwaitingPreparation': 0, 'InPreparation': 1, 'Finished': 2 }
      const aOrder = stateOrder[a.state] ?? 0
      const bOrder = stateOrder[b.state] ?? 0
      return aOrder - bOrder
    })
  return {
    id: x.id,
    name: x.orderReference,
    orderItems: items
  }
}))

async function startOrderItem(itemId) {
  await ks.startOrderItemPreparation(itemId)
  await ks.fetchPendingOrders()
}

async function finishOrderItem(itemId) {
  await ks.finishOrderItem(itemId)
  await ks.fetchPendingOrders()
}
</script>

<template>
  <div class="container mx-auto p-4" data-testid="kitchen-monitor">
    <h1 class="text-2xl font-bold mb-4" data-testid="kitchen-title">Kitchen Work Monitor</h1>
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4" data-testid="orders-container">
      <div 
        v-for="order in pendingOrders" 
        :key="order.id" 
        class="p-4 bg-white dark:bg-gray-800 rounded shadow"
        :data-testid="`order-card-${order.name.toLowerCase()}`">
        <h2 class="text-xl font-semibold mb-2 dark:text-white" :data-testid="`order-title-${order.name.toLowerCase()}`">{{ order.name }} ({{ order.id }})</h2>
        <ul class="space-y-2" :data-testid="`order-items-${order.name.toLowerCase()}`">
          <li 
            v-for="item in order.orderItems" 
            :key="item.id" 
            class="flex items-center justify-between"
            :data-testid="`order-item-${item.id}`"
            :data-order-ref="order.name.toLowerCase()"
            :data-product-name="item.name">
            <span class="dark:text-gray-200">
              <span :data-testid="`item-quantity-${item.id}`">{{ item.quantity }}</span>× 
              <span :data-testid="`item-name-${item.id}`">{{ item.name }}</span>
            </span>
            <div class="space-x-2 flex items-center">
              <span 
                v-if="item.state === 'Finished'" 
                class="text-green-600 dark:text-green-400 font-semibold"
                :data-testid="`item-finished-${item.id}`">Finished</span>
              <template v-else-if="item.state === 'InPreparation'">
                <span
                  class="inline-flex items-center px-2 py-0.5 rounded text-sm font-medium bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200 animate-pulse"
                  :data-testid="`item-in-preparation-${item.id}`">In Progress</span>
                <button 
                  class="px-3 py-1 bg-green-500 hover:bg-green-600 dark:bg-green-600 dark:hover:bg-green-700 text-white rounded" 
                  :data-testid="`finish-item-btn-${item.id}`"
                  @click="finishOrderItem(item.id)">Finish</button>
              </template>
              <button 
                v-else
                class="px-3 py-1 bg-blue-500 hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700 text-white rounded" 
                :data-testid="`start-item-btn-${item.id}`"
                @click="startOrderItem(item.id)">Start</button>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
  
</template>

<style scoped>
/* Tailwind handles styling */
</style>