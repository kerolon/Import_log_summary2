<script setup lang="ts">
    import {Filter} from '@/class/class'   
    const props2 = defineProps({
        filters: Array<Object>,
    })
    const emit = defineEmits(['filter']);
    function filterBy(event: Event) {
        const ddl = event.target as HTMLSelectElement;
        const filter = ddl.value;
        emit('filter',filter);
    }
</script>

<template>
    <div class="filter pos-left">
        <select @change="filterBy" name="ddlInfoTypeFilter" id="ddlInfoTypeFilter">
            <option label="--filter--" value=""></option>
            <template v-for="filter in filters" v-bind:key="filter.id">
                <option v-bind:label=filter.name v-bind:value=filter.val></option>
            </template>
        </select>
    </div>
</template>