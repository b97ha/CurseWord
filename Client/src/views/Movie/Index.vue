<template>
  <div>
    <h2 class="content-block">Movies</h2>

    <div class="content-block">
      <dx-data-grid
        class="dx-card wide-card"
        :data-source="dataSourceConfig"
        :column-auto-width="true"
        :column-hiding-enabled="true"
        key-expr="guid"
      >
        <dx-paging :page-size="10" />
        <dx-pager :show-page-size-selector="true" :show-info="true" />
        <dx-filter-row :visible="true" />

        <DxColumn :width="110" type="buttons">
          <DxButton
            hint="Get Transctiption"
            icon="textdocument"
            name="Transctiption"
            :on-click="getTransctiption"
          />
          <DxButton
            hint="Detect Profanity"
            icon="importselected"
            :on-click="detectProfanity"
          />
        </DxColumn>

        <dx-column data-field="movieName" :hiding-priority="3" />
        <dx-column
          data-field="uploadDate"
          data-type="date"
          format="yyyy/MM/dd"
          :hiding-priority="1"
        />

        <dx-column data-field="isTranscription" :hiding-priority="2" />
      </dx-data-grid>

      <DxLoadPanel
        v-model="loadingVisible"
        :visible="loadingVisible"
        :show-indicator="true"
        :show-pane="true"
        :shading="true"
        :close-on-outside-click="false"
        shading-color="rgba(0,0,0,0.4)"
      />

      <DxPopup
        v-if="transcribe"
        v-model="popupVisible"
        :visible="popupVisible"
        :drag-enabled="false"
        :close-on-outside-click="true"
        :show-close-button="false"
        :show-title="true"
        :max-width="750"
        :max-height="600"
        container=".dx-viewport"
        title="Transcribe"
      >
        <DxToolbarItem
          widget="dxButton"
          toolbar="bottom"
          location="after"
          :options="closeButtonOptions"
        />
        <template #content>
          <DxScrollView width="100%" height="100%">
            <div id="textBlock">
              <p v-for="(trans, i) in transcribe.results.transcripts" :key="i">
                {{ trans.transcript }}
              </p>
            </div>
          </DxScrollView>
        </template>
      </DxPopup>

      <DxPopup
        v-if="movie"
        v-model="popupPlay"
        :visible="popupPlay"
        :drag-enabled="false"
        :close-on-outside-click="true"
        :show-close-button="false"
        :show-title="true"
        :max-width="200"
        :max-height="200"
        container=".dx-viewport"
        title="Play Movie"
      >
        <DxToolbarItem
          widget="dxButton"
          toolbar="bottom"
          location="after"
          :options="playButtonOptions"
        />
        {{ movie.movieName }}
      </DxPopup>
    </div>
  </div>
</template>

<script>
  import { DxLoadPanel } from "devextreme-vue/load-panel";
  import { DxScrollView } from "devextreme-vue/scroll-view";
  import { DxPopup, DxToolbarItem } from "devextreme-vue/popup";
  import DxDataGrid, {
    DxColumn,
    DxFilterRow,

    //DxLookup,
    DxPager,
    DxPaging,
    DxButton,
  } from "devextreme-vue/data-grid";

  export default {
    components: {
      DxPopup,
      DxScrollView,
      DxToolbarItem,
      DxLoadPanel,
      DxDataGrid,
      DxColumn,
      DxFilterRow,
      //DxLookup,
      DxPager,
      DxPaging,
      DxButton,
    },
    data() {
      return {
        dataSourceConfig: null,
        loadingVisible: false,
        positionOf: "",
        transcribe: "",
        popupVisible: false,
        popupPlay: false,
        movie: null,
        closeButtonOptions: {
          text: "Close",
          onClick: () => {
            this.popupVisible = false;
          },
        },
        playButtonOptions: {
          text: "Play",
          stylingMode: "outlined",
          onClick: () => {
            let routeData = this.$router.resolve({
              name: "playMovie",
              params: { fileName: this.movie.fileName },
            });
            window.open(routeData.href, "_blank");
          },
        },
      };
    },
    computed: {
      title() {
        var text = "Play Movie ";
        if (this.movie) {
          console.log(this.movie);
          text += this.movie.movieName;
        }
        return text;
      },
    },
    methods: {
      getMovies() {
        this.$axios
          .get("api/Movies/Get")
          .then((response) => {
            if (response.data.code == 200) {
              this.dataSourceConfig = response.data.result;
            } else {
              this.$notify("Some thing went wrong", "error");
            }
          })
          .catch((e) => {
            this.$notify("Some thing went wrong");
            console.log(e);
          });
      },
      getTransctiption(e) {
        this.loadingVisible = true;
        this.$axios
          .get("api/Movies/GetTranscribe?guid=" + e.row.key)
          .then((response) => {
            console.log(response);
            this.popupVisible = true;
            this.transcribe = response.data.result;
            this.getMovies();
          })
          .catch((e) => {
            this.$notify("Some thing went wrong");
            console.log(e);
          })
          .finally(() => {
            this.loadingVisible = false;
          });
      },
      detectProfanity(e) {
        this.loadingVisible = true;
        this.movie = e.row.data;
        this.$axios
          .get("api/Movies/DetectProfanity?guid=" + e.row.key)
          .then((response) => {
            if (response.data.code == 200) {
              this.popupPlay = true;
            } else {
              this.$notify("Some thing went wrong", "error");
            }
          })
          .catch((ex) => {
            this.$notify("Some thing went wrong", "error");
            console.log(ex);
          })
          .finally(() => {
            this.loadingVisible = false;
          });
      },
    },

    created() {
      this.getMovies();
    },
  };
</script>
