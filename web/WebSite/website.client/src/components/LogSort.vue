<script setup lang="ts">
    
    import {HeaderName} from '@/class/class'
    const props = defineProps({
        headerName: Object
    });
    const emit = defineEmits(['sort']);
    function sortBy(event: Event) {
        const ddlOrder = event.target as HTMLSelectElement;
        const order = ddlOrder.value;
        emit('sort',order);
    }
</script>

<template>
    <div class="order pos-left pad-left">
        <select @change="sortBy" name="ddlOrder" id="ddlOrder">
            <option v-bind:label="headerName.date" value="0"></option>
            <option v-bind:label="headerName.sub_name" value="1"></option>
            <option v-bind:label="headerName.name" value="2"></option>
        </select>
    </div>
</template>
