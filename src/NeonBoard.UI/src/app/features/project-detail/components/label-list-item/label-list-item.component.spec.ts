import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { LabelListItemComponent } from './label-list-item.component';
import { Label } from '../../models/label.model';

initTestEnvironment();

const mockLabel: Label = { id: 'l-1', name: 'Bug', color: 'red' };

describe('LabelListItemComponent', () => {
  let fixture: ComponentFixture<LabelListItemComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LabelListItemComponent],
    });
    fixture = TestBed.createComponent(LabelListItemComponent);
    fixture.componentRef.setInput('label', mockLabel);
    fixture.componentRef.setInput('isEditing', false);
    fixture.componentRef.setInput('isSaving', false);
    fixture.detectChanges();
  });

  it('renders the label name in view mode', () => {
    const labelSpan = fixture.nativeElement.querySelector('span');
    expect(labelSpan.textContent.trim()).toBe('Bug');
  });

  it('emits edit with the label when Edit is clicked', () => {
    let emitted: Label | undefined;
    fixture.componentInstance.edit.subscribe((l: Label) => (emitted = l));

    const editBtn = fixture.nativeElement.querySelector('[aria-label="Edit label"]');
    editBtn.click();

    expect(emitted).toEqual(mockLabel);
  });

  it('emits delete with the labelId when Delete is clicked', () => {
    let emittedId: string | undefined;
    fixture.componentInstance.delete.subscribe((id: string) => (emittedId = id));

    const deleteBtn = fixture.nativeElement.querySelector('[aria-label="Delete label"]');
    deleteBtn.click();

    expect(emittedId).toBe('l-1');
  });
});
