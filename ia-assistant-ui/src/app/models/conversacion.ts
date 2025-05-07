export interface Conversacion {
  pregunta: string;
  respuesta: string;
  textoPlano: string;

}
// chat-model.dto.ts
export interface ChatModelDto {
  id: string;
  nombre: string;
  emoji: string;
}
