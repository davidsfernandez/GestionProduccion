window.seronaCharts = {
    charts: {},
    renderRevenueChart: function (canvasId, dataPoints, labels) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        
        // Gradient Fill
        const gradient = ctx.createLinearGradient(0, 0, 0, 400);
        gradient.addColorStop(0, 'rgba(0, 200, 153, 0.5)'); // Serona Green
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

    renderDoughnutChart: function (canvasId, data, labels, colors) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        
        // Use provided colors or fallback defaults
        const bgColors = colors || [
            '#00C899', '#151628', '#8A94A6', '#3B7DDD', '#fcb92c', '#dc3545'
        ];

        this.charts[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: bgColors,
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
    },

    renderRadarChart: function (canvasId, data, labels) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        this.charts[canvasId] = new Chart(ctx, {
            type: 'radar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Ordens Ativas',
                    data: data,
                    backgroundColor: 'rgba(0, 200, 153, 0.2)', // Transparent Green
                    borderColor: '#00C899',
                    borderWidth: 2,
                    pointBackgroundColor: '#151628', // Dark theme contrast
                    pointBorderColor: '#fff',
                    pointHoverBackgroundColor: '#fff',
                    pointHoverBorderColor: '#00C899',
                    pointRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                elements: {
                    line: { tension: 0.3 } // Smooth lines
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#151628',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        padding: 10,
                        cornerRadius: 8,
                        displayColors: false
                    }
                },
                scales: {
                    r: {
                        angleLines: {
                            color: 'rgba(0, 0, 0, 0.05)' // Subtle lines
                        },
                        grid: {
                            color: 'rgba(0, 0, 0, 0.05)' // Subtle grid
                        },
                        pointLabels: {
                            font: {
                                size: 11,
                                family: "'Inter', sans-serif",
                                weight: '600'
                            },
                            color: '#64748b' // Slate 500
                        },
                        ticks: {
                            display: false, // Hide numeric ticks to reduce clutter
                            backdropColor: 'transparent'
                        },
                        suggestedMin: 0
                    }
                }
            }
        });
    }
};
