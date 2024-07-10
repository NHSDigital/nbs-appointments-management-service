import { MetadataRoute } from "next";

export default function manifest(): MetadataRoute.Manifest {
    return {
        short_name: "NHS Appointments Book",
        name: "NHS Appointments Book",
        icons: [
          {
            src: "favicon.ico",
            sizes: "64x64 32x32 24x24 16x16",
            type: "image/x-icon"
          },
          {
            src: "apple-icon.png",
            type: "image/png",
            sizes: "192x192"
          }
        ],
        start_url: ".",
        display: "standalone",
        theme_color: "#000000",
        background_color: "#ffffff"
      }
}