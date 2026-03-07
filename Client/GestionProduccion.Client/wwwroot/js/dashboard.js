window.dashboardCharts = {
    weeklyChart: null,
    workshopChart: null,

    initWeeklyChart: function (canvasId, labels, data) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.weeklyChart) {
            this.weeklyChart.destroy();
        }

        this.weeklyChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Peças Produzidas',
                    fill: true,
                    backgroundColor: 'rgba(59, 125, 221, 0.1)',
                    borderColor: '#3B7DDD',
                    data: data,
                    tension: 0.4
                }]
            },
            options: {
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: { grid: { display: false } },
                    y: { beginAtZero: true, ticks: { stepSize: 5 } }
                }
            }
        });
    },

    initWorkshopChart: function (canvasId, labels, data) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.workshopChart) {
            this.workshopChart.destroy();
        }

        this.workshopChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Peças',
                    backgroundColor: '#00C899',
                    borderColor: '#00C899',
                    hoverBackgroundColor: '#00C899',
                    hoverBorderColor: '#00C899',
                    data: data,
                    barPercentage: .75,
                    categoryPercentage: .5
                }]
            },
            options: {
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { grid: { display: false }, beginAtZero: true },
                    x: { grid: { display: false } }
                }
            }
        });
    }
};
