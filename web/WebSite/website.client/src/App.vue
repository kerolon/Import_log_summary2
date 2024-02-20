<template>
    <main>
        <div class="container">
            <div>
                <h1 class="title">{{title}}</h1>
            </div>
            <div class="glogin">
                <GoogleLogin :callback="googleLoginCallback" prompt />
            </div>
            <div v-if="data.isLogin">
                <div class="row" v-if="data.logineror">
                    <div>
                        <div>error</div>
                    </div>
                </div>
                <div class="row" v-if="!data.logineror">
                    <div class="row" v-if="!data.ready">
                        <div>
                            <div>Loading...</div>
                        </div>
                    </div>
                    <div class="row" v-if="data.ready">
                        <div>
                            <div>
                                <LogFilter v-bind:filters="filters" @filter="filtering" />
                                <LogSort v-bind:headerName="headerName" @sort="sort" />
                                <LogView v-bind:headerName="headerName" v-bind:logs="data.logs" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div v-if="!data.isLogin">
                <div>
                    <div>plz login</div>
                </div>
            </div>
        </div>
    </main>
</template>

<style scoped>
    .glogin{
        height:4em;
        margin-top:2em;
        text-align:right;
    }
    .title{
        margin-top:0.5em;
    }
</style>
<script lang="ts">
    import { defineComponent, ref } from 'vue';
    import * as signalR from '../node_modules/@microsoft/signalr';
    import LogFilter from '@/components/LogFilter.vue'
    import LogSort from '@/components/LogSort.vue'
    import LogView from '@/components/LogView.vue'
    import { HeaderName, LogData, Filter } from '@/class/class'
    import type { CallbackTypes } from "vue3-google-login"
    import axios from 'axios';
    export default defineComponent({
        components: {
            LogFilter,
            LogSort,
            LogView
        },
        props: ['env'],
        setup(props) {
            const title = ref(props.env.VITE_ENV_TITLE);
            const data = ref({
                logs: [] as Array<LogData>,
                myConnectionId: '' as string,
                ready: false as boolean,
                orderBy: '0' as string,
                filterBy: '' as string,
                isLogin: false as boolean,
                logineror: false as boolean,
            });
            let connection: signalR.HubConnection = null;
            const filtering: (filterBy: string) => void = (filterBy) => {
                if (filterBy != null) {
                    data.value.filterBy = filterBy;
                }
                connection.invoke("reload");
            }

            const googleLoginCallback: CallbackTypes.CredentialCallback = (response) => {
                data.value.isLogin = true;
                axios.get(apiBaseUrl + '/api/GetToken', {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Bearer ' + response.credential
                    }
                })
                    .then((res) => {
                        connection = new signalR.HubConnectionBuilder()
                            .withUrl(apiBaseUrl + '/api', {
                                headers: { "x-ms-signalr-user-id": res.data.item2 },
                                accessTokenFactory: () => {
                                    return res.data.item1;
                                }
                            })
                            .configureLogging(signalR.LogLevel.Information)
                            .withAutomaticReconnect()
                            .build();
                        connection.on('newMessage', onNewMessage);
                        connection.on('newConnection', onNewConnection);
                        connection.start()
                            .then(() => {
                                data.value.ready = true;
                                connection.invoke("reload");
                            })
                            .catch((error) => {
                                data.value.logineror = true;
                                console.error(error);
                            });

                    })
                    .catch((error) => {
                        data.value.logineror = true;
                        console.error(error);
                    });
            };
            function sort(orderby) {
                if (orderby != null) {
                    data.value.orderBy = orderby;
                }
                const by = data.value.orderBy as string;
                data.value.logs.sort((l1, l2) => {
                    if (by == '0') {
                        if (l1.date > l2.date) return 1;
                        else if (l1.date < l2.date) return -1;
                        else return 0;
                    }
                    if (by == '1') {
                        if (l1.sub_name > l2.sub_name) return 1;
                        else if (l1.sub_name < l2.sub_name) return -1;
                        else return 0;
                    }
                    if (by == '2') {
                        if (l1.name > l2.name) return 1;
                        else if (l1.name < l2.name) return -1;
                        else return 0;
                    }
                });
            }

            const headerName = ref<HeaderName>(JSON.parse(props.env.VITE_ENV_HEADER_NAME));

            const filters = ref<Array<Filter>>([]);
            const apiBaseUrl = props.env.VITE_ENV_API_URL;
            
            const onNewMessage = (messages) => {
                if (messages.map) {
                    const logdata = messages.map(message => {
                        return {
                            log_id: message.Id,
                            date: message.Datetime,
                            date_string: message.DatetimeString,
                            info_type: message.TypeString,
                            name: message.Name,
                            sub_name: message.From,
                            description: message.Description,
                            short_description: message.Description.split('\n')[0],
                        } as LogData;
                    });
                    if (data.value.filterBy) {
                        data.value.logs = logdata.filter(l => l.info_type == data.value.filterBy);
                    } else {
                        data.value.logs = logdata;
                    }
                    sort(null);
                    filters.value = Array.from(new Map<string,Filter>(logdata.map(l => [l.info_type, { name: l.info_type, val: l.info_type }])).values()).sort((s1, s2) =>
                    {
                        if (s1.val > s2.val) return 1;
                        else if (s1.val < s2.val) return -1;
                        else return 0;
                    });
                }
            };
            const onNewConnection = (message) => {
                data.value.myConnectionId = message.ConnectionId;
                onNewMessage(message.Logs);
            }
            return {
                title,
                data,
                headerName,
                filters,
                sort,
                filtering,
                googleLoginCallback
            };
        }
    });
</script>

<style scoped>
</style>
