// Частицы на фоне
const canvas = document.getElementById('particles');
const ctx = canvas.getContext('2d');

function resizeCanvas() {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
}
resizeCanvas();
window.addEventListener('resize', resizeCanvas);

const particles = [];
const PARTICLE_COUNT = 80;

class Particle {
    constructor() {
        this.reset();
    }

    reset() {
        this.x = Math.random() * canvas.width;
        this.y = Math.random() * canvas.height;
        this.vx = (Math.random() - 0.5) * 0.5;
        this.vy = (Math.random() - 0.5) * 0.5;
        this.radius = Math.random() * 2 + 1;
        this.opacity = Math.random() * 0.5 + 0.2;
    }

    update() {
        this.x += this.vx;
        this.y += this.vy;

        if (this.x < 0 || this.x > canvas.width) this.vx *= -1;
        if (this.y < 0 || this.y > canvas.height) this.vy *= -1;
    }

    draw() {
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
        ctx.fillStyle = `rgba(168, 85, 247, ${this.opacity})`;
        ctx.fill();
    }
}

for (let i = 0; i < PARTICLE_COUNT; i++) {
    particles.push(new Particle());
}

function animate() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    particles.forEach(p => {
        p.update();
        p.draw();
    });

    // Линии между близкими частицами
    for (let i = 0; i < particles.length; i++) {
        for (let j = i + 1; j < particles.length; j++) {
            const dx = particles[i].x - particles[j].x;
            const dy = particles[i].y - particles[j].y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist < 150) {
                ctx.beginPath();
                ctx.moveTo(particles[i].x, particles[i].y);
                ctx.lineTo(particles[j].x, particles[j].y);
                ctx.strokeStyle = `rgba(168, 85, 247, ${0.15 * (1 - dist / 150)})`;
                ctx.lineWidth = 1;
                ctx.stroke();
            }
        }
    }

    requestAnimationFrame(animate);
}

animate();

// Табы
document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', () => {
        document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
        tab.classList.add('active');
        document.getElementById(tab.dataset.tab).classList.add('active');
    });
});

// Функции для кнопок
async function callBridge(method, ...args) {
    try {
        if (window.chrome && window.chrome.webview) {
            const bridge = window.chrome.webview.hostObjects.sync.bridge;
            return await bridge[method](...args);
        }
    } catch (e) {
        console.error('Bridge error:', e);
    }
    return null;
}

async function loadSkins() {
    try {
        const json = await callBridge('GetSkinsJson');
        if (!json) return;
        const skins = JSON.parse(json);
        const current = await callBridge('GetCurrentSkin') || 'Solid';

        const select = document.getElementById('skinSelect');
        if (!select) return;

        select.innerHTML = skins.map(s =>
            `<option value="${s.name}" ${s.name === current ? 'selected' : ''}>${s.icon} ${s.name}</option>`
        ).join('');

        const display = document.getElementById('currentSkinDisplay');
        if (display) display.textContent = current;
    } catch (e) {
        console.error('loadSkins error:', e);
    }
}

document.getElementById('skinSelect')?.addEventListener('change', async (e) => {
    await callBridge('SetSkin', e.target.value);
    const display = document.getElementById('currentSkinDisplay');
    if (display) display.textContent = e.target.value;
});

function openSettings() { callBridge('OpenSettings'); }
function openSkinsFolder() { callBridge('OpenSkinsFolder'); }
function openGitHub() { callBridge('OpenGitHub'); }

loadSkins();