# Anxiety In Lines Website

This folder contains the local showcase website for the project.

## Page Structure

The website is organized as four pages:

- `Game`: `index.html`
- `Design`: `design.html`
- `Game World`: `game-world.html`
- `Development`: `development.html`

## Run Locally

From the repository root:

```sh
python3 -m http.server 8000
```

Then open:

```text
http://localhost:8000/website/
```

## Add the Unity WebGL Build

When Unity exports the final WebGL version, copy the exported build output into:

```text
website/game/
```

The Unity entry page should be:

```text
website/game/index.html
```

The Game page will automatically embed it in the playable Web version area.

## GitHub Pages

This site is deployed through GitHub Actions using:

```text
.github/workflows/deploy-website.yml
```

The workflow publishes the contents of `website/` as the GitHub Pages artifact. It also copies the root `agent-development-log.md` into the published site so the Development page can load the timeline online.
