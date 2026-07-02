<template>
  <Input
    :model-value="modelValue"
    :placeholder="placeholder"
    class="search-input"
    @update:model-value="$emit('update:modelValue', $event)"
  />
</template>

<script setup>
import { Input } from "@/components/ui/input";

defineProps({
  modelValue: {
    type: String,
    default: "",
  },
  placeholder: {
    type: String,
    default: "Search...",
  },
});

defineEmits(["update:modelValue"]);
</script>

<style scoped>
.search-input {
  width: 100%;
  margin-bottom: 18px;
}
</style>