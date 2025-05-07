import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { ChatModelDto } from '../models/conversacion';

@Injectable({
  providedIn: 'root'
})
export class IaService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  consultarOpenIA(prompt: string, model: string): Observable<string> {
    return this.http.post(`${this.apiUrl}OpenAi`, { prompt, model }, { responseType: 'text' })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          const errorMsg = error.error || 'Error desconocido al comunicarse con la IA.';
          console.error('Error capturado:', errorMsg);
          return of(errorMsg);
        })
      );
  }

  consultarGemini(prompt: string): Observable<string> {
    return this.http.post(`${this.apiUrl}GeminiAi`, { prompt }, { responseType: 'text' }).pipe(
      catchError((error: HttpErrorResponse) => {
        const errorMsg = error.error || 'Error desconocido al comunicarse con Gemini.';
        console.error('Error capturado Gemini:', errorMsg);
        return of(errorMsg);
      })
    );
  }

  generarImagenOpenAi(prompt: string) {
    const body = { prompt };
    return this.http.post<{ imageUrl: string }>(`${this.apiUrl}OpenAi/generar-imagen`, body);
  }

  // generarImagenGemini(prompt: string, imageFile: File | null): Observable<any> {
  generarImagenGemini(payload: any): Observable<any> {
    // const formData = new FormData();
    // formData.append('Prompt', prompt);      // Con mayúscula inicial
    // if (imageFile) {
    //   formData.append('Image', imageFile);  // Con mayúscula inicial      
    // }
    return this.http.post(`${this.apiUrl}GeminiAi/generar-imagenes`, payload);
  }

  getModelos(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}OpenAi`);
  }

  getModelosFiltrados(): Observable<ChatModelDto[]> {
    return this.http.get<ChatModelDto[]>(`${this.apiUrl}OpenAi/obetner-modelos-tratados`);
  }

  descargarImagenDesdeBackend(url: string, parent: any): void {
    const endpoint = `${this.apiUrl}OpenAi/descargar-imagen?url=${encodeURIComponent(url)}`;
    const link = document.createElement('a');
    link.href = endpoint;
    link.download = 'imagen_generada.png';
    link.click();
    parent.cargando = false;
  }
}
