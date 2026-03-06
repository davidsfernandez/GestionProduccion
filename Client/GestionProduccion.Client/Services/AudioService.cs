/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace GestionProduccion.Client.Services
{
    public class AudioService
    {
        private readonly IJSRuntime _js;

        public AudioService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task PlaySuccess() => await PlaySound("success");
        public async Task PlayError() => await PlaySound("error");
        public async Task PlayNotify() => await PlaySound("notify");
        public async Task PlayAction() => await PlaySound("action");

        private async Task PlaySound(string type)
        {
            try
            {
                await _js.InvokeVoidAsync("playUISound", type);
            }
            catch
            {
                // Silence if audio fails or is blocked by browser policy
            }
        }
    }
}

