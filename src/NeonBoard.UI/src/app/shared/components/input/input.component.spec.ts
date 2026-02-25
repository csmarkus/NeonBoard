import { initTestEnvironment } from '../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { InputComponent } from './input.component';

initTestEnvironment();

@Component({
  imports: [InputComponent],
  template: `<app-input [(value)]="testValue" />`,
})
class TestHostComponent {
  testValue = signal('');
}

describe('InputComponent', () => {
  let fixture: ComponentFixture<InputComponent>;
  let component: InputComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [InputComponent] });
    fixture = TestBed.createComponent(InputComponent);
    component = fixture.componentInstance;
  });

  it('renders an input element by default', () => {
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input');
    const textarea = fixture.nativeElement.querySelector('textarea');
    expect(input).toBeTruthy();
    expect(textarea).toBeNull();
  });

  it('renders a textarea when element is textarea', () => {
    fixture.componentRef.setInput('element', 'textarea');
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input');
    const textarea = fixture.nativeElement.querySelector('textarea');
    expect(input).toBeNull();
    expect(textarea).toBeTruthy();
  });

  it('displays the value from the model signal', () => {
    component.value.set('hello');
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.value).toBe('hello');
  });

  it('updates the value model on input event', () => {
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = 'typed';
    input.dispatchEvent(new Event('input'));
    expect(component.value()).toBe('typed');
  });

  it('sets touched to true on blur', () => {
    fixture.detectChanges();
    expect(component.touched()).toBe(false);
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.dispatchEvent(new Event('blur'));
    expect(component.touched()).toBe(true);
  });

  it('disables the input when disabled input is true', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.disabled).toBe(true);
  });

  it('sets maxlength attribute when provided', () => {
    fixture.componentRef.setInput('maxlength', 50);
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.getAttribute('maxlength')).toBe('50');
  });

  it('includes resize-none class for textarea by default', () => {
    fixture.componentRef.setInput('element', 'textarea');
    fixture.detectChanges();
    const textarea: HTMLTextAreaElement = fixture.nativeElement.querySelector('textarea');
    expect(textarea.className).toContain('resize-none');
  });

  it('includes resize-y class when resize is y', () => {
    fixture.componentRef.setInput('element', 'textarea');
    fixture.componentRef.setInput('resize', 'y');
    fixture.detectChanges();
    const textarea: HTMLTextAreaElement = fixture.nativeElement.querySelector('textarea');
    expect(textarea.className).toContain('resize-y');
  });

  it('displays validation errors when touched and invalid', () => {
    fixture.componentRef.setInput('invalid', true);
    fixture.componentRef.setInput('errors', [{ kind: 'required', message: 'Field is required' }]);
    component.touched.set(true);
    fixture.detectChanges();
    const errorSpan = fixture.nativeElement.querySelector('.text-danger');
    expect(errorSpan).toBeTruthy();
    expect(errorSpan.textContent.trim()).toBe('Field is required');
  });

  it('does not display validation errors when not touched', () => {
    fixture.componentRef.setInput('invalid', true);
    fixture.componentRef.setInput('errors', [{ kind: 'required', message: 'Field is required' }]);
    fixture.detectChanges();
    const errorSpan = fixture.nativeElement.querySelector('.text-danger');
    expect(errorSpan).toBeNull();
  });

  it('applies error styles when touched and invalid', () => {
    fixture.componentRef.setInput('invalid', true);
    component.touched.set(true);
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.className).toContain('border-danger');
  });
});

describe('InputComponent with model binding', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [TestHostComponent] });
    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
  });

  it('syncs value from host signal to input', () => {
    host.testValue.set('from-host');
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.value).toBe('from-host');
  });

  it('syncs value from input back to host signal', () => {
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = 'from-view';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(host.testValue()).toBe('from-view');
  });
});
