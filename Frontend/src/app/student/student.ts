import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { WebsocketService } from '../services/websocket.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-student',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './student.html',
  styleUrl: './student.css',
})
export class Student implements OnInit {

  students: any[] = [];
  courses: any[] = [];
  logs: string[] = [];

  private connected = false;

  constructor(
    private ws: WebsocketService,
    private zone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {

    if (this.connected) return;
    this.connected = true;

    this.ws.connect((msg, eventType) => {

      if (eventType === "OPEN") {
      console.log("Dashboard connected to live server");
      return;
      }

      console.log("Received", msg);

      // READ responses
      if (msg.eventType === "READ") {

        if (msg.entity === "Student")
          this.students = msg.data;

        if (msg.entity === "Course")
          this.courses = msg.data;

        this.cdr.detectChanges();
        return;
      }

      // logs
      if (msg.eventType && msg.entity) {
        this.logs.unshift(`${msg.eventType} → ${msg.entity}`);
        if (this.logs.length > 15) this.logs.pop();
      }

      // entity handlers
      if (msg.entity === "Student") this.handleStudent(msg);
      if (msg.entity === "Course") this.handleCourse(msg);

      // force UI refresh
      this.cdr.detectChanges();
    });
  }

  handleStudent(msg: any) {

    if (msg.eventType === "CREATE") {
      this.students = [...this.students, msg.data];
    }

    if (msg.eventType === "UPDATE") {
      this.students = this.students.map(s =>
        s.StudentId === msg.data.StudentId ? msg.data : s
      );
    }

    if (msg.eventType === "DELETE") {
      this.students = this.students.filter(
        s => s.StudentId !== msg.data.StudentId
      );
    }
  }

  handleCourse(msg: any) {

    if (msg.eventType === "CREATE") {
      this.courses = [...this.courses, msg.data];
    }

    if (msg.eventType === "UPDATE") {
      this.courses = this.courses.map(c =>
        c.CourseId === msg.data.CourseId ? msg.data : c
      );
    }

    if (msg.eventType === "DELETE") {
      this.courses = this.courses.filter(
        c => c.CourseId !== msg.data.CourseId
      );
    }
  }
}