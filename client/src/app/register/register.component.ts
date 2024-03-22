import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../services/account.service';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  registerForm: FormGroup = new FormGroup({});
  maxDate: Date = new Date();
  validationErrors: string[] | undefined;
  constructor(private accountService: AccountService,
    private toastrService: ToastrService,
    private formBuilder: FormBuilder,
    private router: Router) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm() {
    this.registerForm = this.formBuilder.group({
      username: ['', Validators.required],
      gender: ['male'],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(8)]],
      confirmPassword: ['', [
        Validators.required,
        this.matchValues('password')]],
    });
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () => this.registerForm
        .controls['confirmPassword'].updateValueAndValidity()
    })
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control
        .parent?.get(matchTo)?.value ? null : {
        notMatching: true
      };
    };
  }

  register() {
    const onlyDateOfBirth = this
      .getDateOnly(this.registerForm.controls['dateOfBirth'].value);
    const registerFormValues = {
      ...this.registerForm.value,
      dateOfBirth: onlyDateOfBirth
    };
    this.accountService.register(registerFormValues)
      .subscribe({
        next: () => {
          this.router.navigateByUrl('/members');
        }, error: (error) => {
          this.validationErrors = error;
        }
      });
  }

  private getDateOnly(dateOfBirth: string | undefined) {
    if (!dateOfBirth) { return; }
    let theDateOfBirth = new Date(dateOfBirth);
    return new Date(theDateOfBirth.setMinutes(
      theDateOfBirth.getMinutes() - theDateOfBirth.getTimezoneOffset()))
      .toISOString().slice(0, 10);
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
