import draggable from 'vuedraggable'
import dashboardServices from "@/services/dashboard-services.js";
import contextMenu from 'vue-context-menu'
export default {
    name: 'Dashboard',
    components: {
        draggable,
        contextMenu
    },
    data: () => ({
        selected: ['A', 'B', 'C', 'D', 'E'],
        headers: [],
        mods: [],
        detail: null,
        showDialog: false,
        sort: false,
        active: false,
        newMenuData: {},
        options: [
            { item: 'A', name: 'Active', notEnabled: true },
            { item: 'B', name: 'Order', notEnabled: true },
            { item: 'C', name: 'Name' },
            { item: 'D', name: 'Author' },
            { item: 'E', name: 'Categories' },
            { item: 'F', name: 'Sort' }
        ]

    }),
    created() {
        this.fetchData();
    },
    methods: {
        Detail(item) {
            this.detail = item;
        },
        fetchData() {

            dashboardServices.Get().then((result) => {
                this.mods = result.data;
            })
            this.headers = [{
                    text: 'order'

                },
                {
                    text: 'Mod name',
                    align: 'start',
                    value: 'name',
                },
                { text: 'Author', value: 'author' },
                { text: 'Category', value: 'category' },
            ]

            for (let i = 0; i < 10; i++) {
                this.mods.push({ order: i, name: `mod_${i}`, author: `author ${i}`, category: `cat 1, cat 2, cat 3` })
            }

        },
        CheckModPage_Click() {},
        ActiveMods() {},
        DeactiveMods() {},
        ToggleActive() {},
    },
    mounted() {}
}