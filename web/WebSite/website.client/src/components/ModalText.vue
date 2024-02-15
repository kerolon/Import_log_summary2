<script setup lang="ts">
    const props2 = defineProps({
        title:String,
        text: String,
        showModal: Boolean
    });
    const emit = defineEmits(['closeModal']);
</script>

<template>
    <Transition name="bg">
        <div @click.self="$emit('closeModal')" class="modal-bg" v-show="showModal">
            <Transition name="content">
                <div class="modal-content" v-show="showModal">
                    <div class="modal-header">{{title}}</div>
                    <p class="modal-text">{{text}}</p>
                    <div class="modal-footer">
                        <button class="btn" @click="$emit('closeModal')">close</button>
                    </div>
                </div>
            </Transition>
        </div>
    </Transition>
</template>

<style scoped>
    .modal-bg{
        position:fixed;
        top:0;
        left:0;
        bottom:0;
        right:0;
        background-color:rgba(0,0,0,0.4);
    }
    .modal-content {
        margin: 2em auto;
        width: 40em;
        padding: 1em;
        border:0;
        box-shadow: 0 0 1em;
    }
    .modal-footer{

    }
    .modal-header {
        padding: 0 0 1em 0;
    }
    .modal-text {
        padding-top: 1em;
        max-height: 70vh;
        overflow-y: auto;
        white-space: pre-line;
    }
    .btn{
        border: 1px solid #000;
    }
    .btn:hover{
        background-color: #888;
        color: #fff;
    }

    .content-enter-from {
        opacity: 0;
        transform: translateY(-110%);
    }

    .content-enter-active {
        transition: all 0.3s ease-out;
    }

    .content-enter-to {
        opacity: 1;
        transform: translateY(0%);
    }
    .bg-enter-from {
        opacity: 0;
    }

    .bg-enter-active {
        transition: all 0.2s ease-out;
    }

    .bg-enter-to {
        opacity: 0.4;
    }
</style>