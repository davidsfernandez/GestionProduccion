/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * Serona BI - Chart.js Integration
 */

window.renderBonusChart = (report) => {
    const ctx = document.getElementById('bonusChart');
    if (!ctx) return;

    // Destroy existing chart if it exists
    const existingChart = Chart.getChart(ctx);
    if (existingChart) existingChart.destroy();

    new Chart(ctx, {
        type: 'radar',
        data: {
            labels: ['Produtividade', 'Qualidade', 'Prazo (Efic.)'],
            datasets: [{
                label: 'Desempenho Real %',
                data: [
                    report.productivityPercentage, 
                    100 - report.defectPercentage, 
                    report.deadlinePerformance
                ],
                backgroundColor: 'rgba(0, 200, 153, 0.2)',
                borderColor: 'rgba(0, 200, 153, 1)',
                borderWidth: 3,
                pointBackgroundColor: 'rgba(0, 200, 153, 1)',
                fill: true
            }, {
                label: 'Meta Ideal',
                data: [100, 100, 100],
                backgroundColor: 'rgba(59, 125, 221, 0.05)',
                borderColor: 'rgba(59, 125, 221, 0.3)',
                borderDash: [5, 5],
                borderWidth: 1,
                fill: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                r: {
                    angleLines: { display: true },
                    suggestedMin: 0,
                    suggestedMax: 100,
                    ticks: { display: false }
                }
            },
            plugins: {
                legend: { position: 'bottom' }
            }
        }
    });
};
