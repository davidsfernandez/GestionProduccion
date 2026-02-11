window.seronaCharts = {
    renderRevenueChart: function (canvasId, dataPoints, labels) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        
        // Gradient Fill
        const gradient = ctx.createLinearGradient(0, 0, 0, 400);
        gradient.addColorStop(0, 'rgba(0, 200, 153, 0.5)'); // Serona Green
        gradient.addColorStop(1, 'rgba(0, 200, 153, 0.0)');

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Produção',
                    data: dataPoints,
                    backgroundColor: gradient,
                    borderColor: '#00C899',
                    borderWidth: 2,
                    pointBackgroundColor: '#ffffff',
                    pointBorderColor: '#00C899',
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    fill: true,
                    tension: 0.4 // Smooth curves
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: '#151628',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        displayColors: false,
                        padding: 10,
                        cornerRadius: 8
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: '#8A94A6'
                        }
                    },
                    y: {
                        grid: {
                            color: '#f0f0f0',
                            borderDash: [5, 5]
                        },
                        ticks: {
                            color: '#8A94A6',
                            beginAtZero: true,
                            stepSize: 1
                        },
                        border: {
                            display: false
                        }
                    }
                }
            }
        });
    },

    renderDoughnutChart: function (canvasId, data, labels) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: [
                        '#00C899', // Serona Green
                        '#151628', // Dark
                        '#8A94A6', // Gray
                        '#3B7DDD'  // Blue
                    ],
                    borderWidth: 0,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            boxWidth: 10,
                            usePointStyle: true
                        }
                    }
                },
                cutout: '70%'
            }
        });
    },

    renderBarChart: function (canvasId, data, labels) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Qtd',
                    data: data,
                    backgroundColor: '#00C899',
                    borderRadius: 4,
                    barThickness: 20
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { borderDash: [5, 5] }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }
};
