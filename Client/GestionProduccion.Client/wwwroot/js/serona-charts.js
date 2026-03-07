/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

window.seronaCharts = {
    charts: {},
    renderRevenueChart: function (canvasId, dataPoints, labels) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        
        const gradient = ctx.createLinearGradient(0, 0, 0, 400);
        gradient.addColorStop(0, 'rgba(0, 200, 153, 0.3)'); 
        gradient.addColorStop(1, 'rgba(0, 200, 153, 0.0)');

        this.charts[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Produção',
                    data: dataPoints,
                    backgroundColor: gradient,
                    borderColor: '#00C899',
                    borderWidth: 3,
                    pointBackgroundColor: '#ffffff',
                    pointBorderColor: '#00C899',
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#151628',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        displayColors: false,
                        padding: 12,
                        cornerRadius: 10
                    }
                },
                scales: {
                    x: { grid: { display: false }, ticks: { color: '#8A94A6', font: { size: 11 } } },
                    y: { grid: { color: '#f0f0f0', borderDash: [5, 5] }, ticks: { color: '#8A94A6', beginAtZero: true }, border: { display: false } }
                }
            }
        });
    },

    renderBarChart: function (canvasId, data, labels) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        
        this.charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Peças',
                    data: data,
                    backgroundColor: '#00C899',
                    borderRadius: 6,
                    barThickness: 30
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, grid: { borderDash: [5, 5], color: '#f0f0f0' }, border: { display: false } },
                    x: { grid: { display: false } }
                }
            }
        });
    }
};
