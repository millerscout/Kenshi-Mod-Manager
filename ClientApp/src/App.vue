<template>
  <div class="container p-5">
    <header>
      <center>
        <h1>Kenshi Mod Tool</h1>
      </center>
    </header>
    <section>
      <div>
        <b-navbar toggleable="lg" type="dark" variant="info">
          <b-collapse id="nav-collapse" is-nav>
            <b-navbar-nav class="ml-auto">
              <b-nav-item @click="ShowConfig()">
                <b-icon icon="gear-fill" aria-hidden="true"></b-icon>&nbsp;Settings
              </b-nav-item>
            </b-navbar-nav>
          </b-collapse>
        </b-navbar>
      </div>
      <Dashboard />
      <confirmDialog />
    </section>
    <b-modal id="mar-center" centered v-model="modalShow" title="Config" size="lg">
      <div class="form-group mb-0">
        <label for="txtGamePath">Game Path</label>
        <input
          id="txtGamePath"
          type="text"
          class="form-control"
          value="C:\Program Files (x86)\Steam\steamapps\common\Kenshi"
        />
      </div>

      <div class="form-group mb-0">
        <label for="txtGamePath">Steam Mod Folder</label>
        <input
          id="txtSteamModsPath"
          type="text"
          class="form-control"
          value="C:\Program Files (x86)\Steam\steamapps\workshop\content\233860"
        />
      </div>
      <div class="form-group mb-0">
        <label for="txtGamePath">Steam Url Page</label>
        <input
          id="txtSteamPageUrl"
          type="text"
          class="form-control"
          value="https://steamcommunity.com/sharedfiles/filedetails/?id="
        />
      </div>
      <div class="form-group mb-0">
        <label for="txtGamePath">Nexus Url Page</label>

        <input
          id="txtNexusPageUrl"
          type="text"
          class="form-control"
          value="https://www.nexusmods.com/kenshi/search/?gsearch="
        />
      </div>
    </b-modal>
  </div>
</template>

<script>
import Dashboard from "./components/Dashboard";
import confirmDialog from "./helpers/confirmDialog";

export default {
  name: "app",
  components: {
    Dashboard,
    confirmDialog
  },
  data() {
    return {
      modalShow: false
    };
  },
  methods: {
    ShowConfig() {
      this.modalShow = true;
    }
  }
};
</script>

<style lang="scss">
@import "./assets/styles/variables";
@import "./assets/styles/bootstrap";
</style>