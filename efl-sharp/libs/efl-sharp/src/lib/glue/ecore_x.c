#include <stdlib.h>
#include <Ecore.h>
#include <Ecore_X.h>
#include <Ecore_X_Atoms.h>

int _ecore_x_mouse_button_down()
{
   return ECORE_X_EVENT_MOUSE_BUTTON_DOWN;
}

int _ecore_x_mouse_button_up()
{
   return ECORE_X_EVENT_MOUSE_BUTTON_UP;
}

int _ecore_x_mouse_move()
{
	return ECORE_X_EVENT_MOUSE_MOVE;
}

int _ecore_x_mouse_in()
{
	return ECORE_X_EVENT_MOUSE_IN;
}

int _ecore_x_mouse_out()
{
	return ECORE_X_EVENT_MOUSE_OUT;
}

int _ecore_x_dnd_enter()
{
   return ECORE_X_EVENT_XDND_ENTER;
}

int _ecore_x_dnd_position()
{
   return ECORE_X_EVENT_XDND_POSITION;
}

int _ecore_x_dnd_status()
{
   return ECORE_X_EVENT_XDND_STATUS;
}

int _ecore_x_dnd_leave()
{
   return ECORE_X_EVENT_XDND_LEAVE;
}

int _ecore_x_dnd_drop()
{
   return ECORE_X_EVENT_XDND_DROP;
}

int _ecore_x_dnd_finished()
{
   return ECORE_X_EVENT_XDND_FINISHED;
}

unsigned int _ecore_x_dnd_action_private()
{
   return ECORE_X_DND_ACTION_PRIVATE;
}

int _ecore_x_selection_notify()
{
   return ECORE_X_EVENT_SELECTION_NOTIFY;
}


