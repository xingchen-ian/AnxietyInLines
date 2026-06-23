const gameFrame = document.querySelector("#gameFrame");
const gameFallback = document.querySelector("#gameFallback");
const webglStatus = document.querySelector("#webglStatus");
const timeline = document.querySelector("#timeline");

async function checkWebBuild() {
  if (!gameFrame || !gameFallback || !webglStatus) return;

  try {
    const response = await fetch("game/index.html", { method: "HEAD" });
    if (response.ok) {
      webglStatus.textContent = "Available";
      gameFallback.style.display = "none";
      gameFrame.style.display = "block";
      return;
    }
  } catch (error) {
    // Local file access or missing dev server can make HEAD unavailable.
  }

  webglStatus.textContent = "Not exported yet";
  gameFrame.style.display = "none";
  gameFallback.style.display = "grid";
}

function getSection(text, heading) {
  const start = text.indexOf(heading);
  if (start === -1) return "";
  const next = text.indexOf("\n### ", start + heading.length);
  return text
    .slice(start + heading.length, next === -1 ? undefined : next)
    .trim();
}

function summarizeEntry(entry) {
  const title = entry.match(/## (Interaction \d+).*/);
  const stage = entry.match(/\*\*Development Stage:\*\*\s*(.*)/);
  const goal = entry.match(/\*\*Current Goal:\*\*\s*(.*)/);
  const result = getSection(entry, "### Immediate Result");

  return {
    number: title ? title[1] : "Interaction",
    stage: stage ? stage[1].trim() : "Development update",
    goal: goal ? goal[1].trim() : "",
    result: result.split("\n").filter(Boolean)[0] || ""
  };
}

async function loadTimeline() {
  if (!timeline) return;

  try {
    let response = await fetch("agent-development-log.md");
    if (!response.ok) {
      response = await fetch("../agent-development-log.md");
    }
    if (!response.ok) throw new Error("Log not found");
    const text = await response.text();
    const entries = text
      .split("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
      .filter((entry) => entry.includes("Raw Interaction Log"))
      .map(summarizeEntry);

    if (!entries.length) throw new Error("No timeline entries found");

    timeline.innerHTML = entries
      .map((entry) => `
        <article class="timeline-card">
          <div class="timeline-number">${entry.number}</div>
          <div>
            <h3>${entry.stage}</h3>
            <p>${entry.goal}</p>
            ${entry.result ? `<p>${entry.result}</p>` : ""}
          </div>
        </article>
      `)
      .join("");
  } catch (error) {
    timeline.innerHTML = `
      <article class="timeline-card">
        <div class="timeline-number">Log</div>
        <div>
          <h3>Development log unavailable</h3>
          <p>Run this site through a local web server from the project root so the page can load agent-development-log.md.</p>
        </div>
      </article>
    `;
  }
}

checkWebBuild();
loadTimeline();
