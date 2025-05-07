import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { IaService } from '../../services/ia.service';
import { ChatModelDto, Conversacion } from '../../models/conversacion';
import { marked } from 'marked';

@Component({
  selector: 'app-ia-chat',
  templateUrl: './ia-chat.component.html',
  styleUrl: './ia-chat.component.css'
})
export class IaChatComponent implements OnInit {
  prompt = '';
  historial: Conversacion[] = [];
  iaSeleccionada: 'openai' | 'gemini' = 'openai';
  idiomaSeleccionado: string = 'es';
  modelosFiltrado: ChatModelDto[] = [];
  modelos: string[] = [];
  modeloSeleccionado: string = '';
  cargando = false;

  // Para mostrar la imagen generada
  imagenGeneradaUrl: string | null = null;
  // Para la imagen que el usuario puede seleccionar
  selectedImageFile: File | null = null;
  generarImagen: boolean = false;

  imagenVistaAmpliada: string | null = null;

  @ViewChild('scrollArea') scrollArea!: ElementRef;
  constructor(private iaService: IaService) { }

  ngOnInit() {
    const guardado = localStorage.getItem('historialIA');
    if (guardado) {
      this.historial = JSON.parse(guardado);
      setTimeout(() => {
        this.scrollToBottom();
      }, 100);

    }

    this.loadModelo();
    window.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && this.imagenVistaAmpliada) {
        this.cerrarVistaAmpliada();
      }
    });
  }
  
  nuevoChat() {
    this.historial = [];
    localStorage.removeItem('historialIA');
    this.prompt = '';
    this.iaSeleccionada = 'openai';     // valor por defecto
    this.idiomaSeleccionado = 'es';     // ajusta según tu modelo

    this.imagenGeneradaUrl = null; // Limpiar la imagen al iniciar un nuevo chat
    this.selectedImageFile = null;
    this.generarImagen = false;
    this.imagenVistaAmpliada = null;
  }

  scrollToBottom() {
    if (this.scrollArea) {
      try {
        this.scrollArea.nativeElement.scrollTop = this.scrollArea.nativeElement.scrollHeight;
      } catch (err) {
        console.warn('No se pudo hacer scroll al final:', err);
      }
    }
  }

  onFileSelected(event: any): void {
    this.selectedImageFile = event.target.files?.[0] || null;
  }

  procesarRespuestaTexto(respuesta: string) {
    const respuestaLimpia = respuesta
      .replace(/\r\n/g, '\n')
      .replace(/\n{3,}/g, '\n\n')
      .trim();
    Promise.resolve(marked.parse(respuestaLimpia)).then((htmlRespuesta: string) => {
      this.historial.push({ pregunta: this.prompt, respuesta: htmlRespuesta, textoPlano: respuestaLimpia });
    });
  }
  private obtenerImagenOpenIA(pregunta: string) {
    this.iaService.generarImagenOpenAi(pregunta).subscribe({
      next: (respuesta) => {
        this.cargando = false;
        const url = respuesta.imageUrl;
        this.historial.push({
          pregunta,
          respuesta: `<img class="imagen-generada" src="${url}" alt="Imagen generada por DALL·E">`,
          textoPlano: 'Imagen generada'
        });
        localStorage.setItem('historialIA', JSON.stringify(this.historial));
        setTimeout(() => this.scrollToBottom(), 100);
      },
      error: (error) => {
        this.cargando = false;
        console.error('Error al generar imagen:', error);
        this.historial.push({ pregunta: this.prompt, respuesta: `Error al generar imagen: ${error.message}`, textoPlano: `Error al generar imagen: ${error.message}` });
        localStorage.setItem('historialIA', JSON.stringify(this.historial));
        setTimeout(() => this.scrollToBottom(), 100);
      }
    });
  }
  private async obtenerEnviarImagenGemini(dto: any, pregunta: string) {
    this.iaService.generarImagenGemini(dto).subscribe(async (respuesta) => {
      this.cargando = false;

      const partes = respuesta?.candidates?.[0]?.content?.parts || [];

      let texto = '';
      let base64 = '';

      for (const parte of partes) {
        if (parte.text) {
          texto = parte.text;
        }
        if (parte.inlineData?.data) {
          base64 = parte.inlineData.data;
        }
      }

      if (base64) {
        this.imagenGeneradaUrl = `data:image/png;base64,${base64}`;
        this.historial.push({
          pregunta,
          respuesta: `<p>${texto}</p><img class="imagen-generada" src="${this.imagenGeneradaUrl}" alt="Imagen generada por Gemini" style="max-width: 100%; height: auto; max-height: 250px; display: block; margin: 10px auto; border-radius: 10px;">`,
          textoPlano: 'Imagen generada'
        });
      } else {
        // Solo texto
        const htmlRespuesta = (await marked.parse(texto)).replace(/^\s*<(p|ul|ol|div)[^>]*>/i, '<span>').replace(/<\/(p|ul|ol|div)>/, '</span>');
        this.historial.push({ pregunta, respuesta: htmlRespuesta, textoPlano: texto });
      }

      localStorage.setItem('historialIA', JSON.stringify(this.historial));
      setTimeout(() => this.scrollToBottom(), 100);
    }, error => {
      this.cargando = false;
      console.error('Error al generar imagen:', error);
      this.historial.push({ pregunta, respuesta: `Error al generar imagen: ${error}`, textoPlano: `Error al generar imagen: ${error}` });
      localStorage.setItem('historialIA', JSON.stringify(this.historial));
      setTimeout(() => this.scrollToBottom(), 100);
    });
  }

  enviar() {
    if (!this.prompt.trim()) return;

    this.cargando = true;
    this.scrollToBottom();

    const idiomaPrompt = this.idiomaSeleccionado === 'es' ? 'en español' :
      this.idiomaSeleccionado === 'en' ? 'in English' :
        this.idiomaSeleccionado === 'fr' ? 'en français' :
          this.idiomaSeleccionado === 'de' ? 'auf Deutsch' : '';

    const pregunta = this.prompt.trim(); 
    // guarda lo que escribió el usuario
    this.prompt = '';

    if (this.iaSeleccionada === 'gemini' && this.generarImagen) {
      if (this.selectedImageFile) {
        const reader = new FileReader();
        reader.onload = () => {
          // quita el "data:image/png;base64,"
          const base64 = (reader.result as string).split(',')[1]; 
          const mimeType = this.selectedImageFile?.type || 'image/png';
          const dto = {
            prompt: pregunta,
            model: null,
            imagePath: base64,
            mimeType: mimeType // ⬅️ esto es importante

          };

          this.obtenerEnviarImagenGemini(dto, pregunta/*  */);
        };
        reader.readAsDataURL(this.selectedImageFile);
      } else {
        const dto = {
          prompt: pregunta,
          model: null,
          imagePath: null
        };

        this.obtenerEnviarImagenGemini(dto, pregunta);
      }
    } else if (this.iaSeleccionada === 'openai' && this.generarImagen) {
      this.obtenerImagenOpenIA(pregunta);
    }
    else {
      const preguntaParaIA = `${pregunta}\nPor favor, responde ${idiomaPrompt}.`;

      this.prompt = '';

      const llamadaIA = this.iaSeleccionada === 'openai'
        ? this.iaService.consultarOpenIA(preguntaParaIA, this.modeloSeleccionado)
        : this.iaService.consultarGemini(preguntaParaIA);

      llamadaIA.subscribe(async (respuesta) => {
        const respuestaLimpia = respuesta
          .replace(/\r\n/g, '\n')
          .replace(/\n{3,}/g, '\n\n') // más de 2 saltos = solo 2
          .trim();

        const htmlRespuesta = (await marked.parse(respuestaLimpia))
          .replace(/^\s*<(p|ul|ol|div)[^>]*>/i, '<span>') // reemplaza primer tag de bloque por <span>
          .replace(/<\/(p|ul|ol|div)>/, '</span>');
        this.historial.push({ pregunta, respuesta: htmlRespuesta, textoPlano: respuestaLimpia });
        localStorage.setItem('historialIA', JSON.stringify(this.historial));
        this.cargando = false;
        setTimeout(() => {
          this.scrollArea.nativeElement.scrollTop = this.scrollArea.nativeElement.scrollHeight;
        }, 100);
      });
    }
  }

  loadModelo() {
    this.iaService.getModelosFiltrados().subscribe(data => {
      this.modelosFiltrado = data;
      if (data.length > 0) this.modeloSeleccionado = data[0].id;
    });

    this.iaService.getModelos().subscribe(data => {
      this.modelos = data;
      this.modeloSeleccionado = 'gpt-3.5-turbo';
    });
  }

  copiarTexto_(texto: string) {
    const tempElement = document.createElement('textarea');
    tempElement.value = texto.replace(/<[^>]+>/g, ''); // quitar HTML si lo deseas
    document.body.appendChild(tempElement);
    tempElement.select();
    document.execCommand('copy');
    document.body.removeChild(tempElement);
  }

  copiarHtmlComoEstilo(elemento: HTMLElement) {
    const html = elemento.innerHTML;
    const blob = new Blob([html], { type: 'text/html' });
    const data = [new ClipboardItem({ 'text/html': blob })];

    navigator.clipboard.write(data).then(() => {
      console.log('Contenido HTML copiado con estilo');
    });
  }


  copiarTexto(texto: string) {
    navigator.clipboard.writeText(texto).then(() => {
      console.log('Texto copiado');
    });
  }

  verImagen(respuesta: string) {
    const src = this.extraerSrc(respuesta);
    this.imagenVistaAmpliada = src;
    /*const ventana = window.open();
    if (ventana) {
      ventana.document.write(`<img src="${src}" style="max-width: 100%; height: auto;" />`);
    }*/
  }

  descargarImagen(respuesta: string) {
    this.cargando = true;
    const src = this.extraerSrc(respuesta);

    if (src.startsWith('data:')) {
      // Base64: descarga directa
      const a = document.createElement('a');
      a.href = src;
      a.download = 'imagen_generada.png';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      this.cargando = false;
    } else {
      this.iaService.descargarImagenDesdeBackend(src, this);
    }
  }

  extraerSrc(respuesta: string): string {
    const match = respuesta.match(/<img[^>]+src="([^">]+)"/);
    return match?.[1] || '';
  }

  cerrarVistaAmpliada() {
    this.imagenVistaAmpliada = null;
  }
}