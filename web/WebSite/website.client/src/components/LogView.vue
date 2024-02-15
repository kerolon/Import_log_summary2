<script lang="ts">
    import { HeaderName, LogData } from '@/class/class'
    import ModalText from '@/components/ModalText.vue'
    import { defineComponent , ref } from 'vue';

    export default defineComponent(
        {
            components: {
                ModalText
            },
            props: {
                headerName: {
                    type: Object as PropType<HeaderName>,
                },
                logs: {
                    type: Array as PropType<Array<LogData>>,
                }
            },
            setup(props, context) {
                const showModal =(text: string) => {
                    modalText.value = text;
                    isShowModal.value = true;
                }
                const closeModal = () => {
                    isShowModal.value = false;
                }
                const modalText = ref('');
                const isShowModal = ref(false);
                return {
                    closeModal,
                    showModal,
                    modalText,
                    isShowModal
            }
        }
    }) 
</script>


<template>
    <div class="table-responsive">
        <table id="main" class="table table-striped table-borderd table-hover">
            <thead>
                <tr>
                    <td id="date_head">
                        {{headerName.date}}
                    </td>
                    <td id="infoType_head">
                        {{headerName.info_type}}
                    </td>
                    <td id="name_head">
                        {{headerName.name}}
                    </td>
                    <td id="subName_head">
                        {{headerName.sub_name}}
                    </td>
                    <td id="description_head">
                        {{headerName.description}}
                    </td>
                </tr>
            </thead>
            <tbody>
                <template v-for="log in logs" v-bind:key="log.log_id">
                    <tr class="clickable" @click="showModal(log.description)">
                        <td>
                            {{log.date_string}}
                        </td>
                        <td>
                            {{log.info_type}}
                        </td>
                        <td>
                            {{log.name}}
                        </td>
                        <td c>
                            {{log.sub_name}}
                        </td>
                        <td>
                            {{log.short_description}}
                        </td>
                    </tr>
                </template>
            </tbody>
        </table>
        <ModalText v-bind:showModal="isShowModal" @close-modal="closeModal" v-bind:text="modalText" v-bind:title="headerName.description" />
    </div>
</template>

<style scoped>
    div.descript {
        white-space: pre-line;
    }
    #date_head{
        width:9em;
    }
    #infoType_head {
        width: 7em;
    }
    #subName_head{
        width:6em;
    }
    #description_head{
        width:25em;
    }
    table{
        width:80vw;
    }
    .clickable{
        cursor:pointer;
    }
</style>