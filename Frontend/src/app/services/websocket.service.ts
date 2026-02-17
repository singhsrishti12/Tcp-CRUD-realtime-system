import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WebsocketService {

  private socket!: WebSocket;
  private url = "ws://localhost:5225/ws";
  private reconnectInterval = 1000; // start with 1s
  private maxReconnectInterval = 10000;
private reconnecting = false;
  connect(callback: (msg:any, event?:string)=>void) {
    if (this.socket && this.socket.readyState === WebSocket.OPEN)
      return;

    this.socket = new WebSocket(this.url);

    this.socket.onopen = () => {
      console.log("Connected");
      callback(null, "OPEN");
      this.reconnectInterval = 1000; // reset interval after successful connection
    };

    this.socket.onmessage = (event) => {
      try {
        const parsed = JSON.parse(event.data);
        callback(parsed);
      } catch (err) {
        console.error("Invalid JSON:", event.data);
      }
    };

    this.socket.onerror = (err) => {
      console.error("WebSocket error:", err);
    };

    // this.socket.onclose = (event) => {
    //   console.warn("WebSocket closed", event.reason);

    //   // Try to reconnect
    //   setTimeout(() => {
    //     console.log("Reconnecting...");
    //     this.reconnectInterval = Math.min(this.reconnectInterval * 2, this.maxReconnectInterval);
    //     this.connect(callback);
    //   }, this.reconnectInterval);
    // };
    

this.socket.onclose = (event) => {
  if (!this.reconnecting) {
    this.reconnecting = true;
    console.warn("WebSocket closed. Reconnecting...");
    setTimeout(() => {
      this.reconnecting = false; // allow future reconnects
      this.connect(callback);
    }, this.reconnectInterval);
  }
};

  }

  send(message: string) {
    if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
      console.warn("Socket not ready. Message ignored:", message);
      return;
    }
    this.socket.send(message);
  }
}